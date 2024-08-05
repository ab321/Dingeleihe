using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Entities;
using Core.Entities.DTO;
using Core.Entities.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence;

namespace Dingeleihe_WebApi;

public class LendingController
{
    public LendingController(ApplicationDbContext context, RentalValidator rentalValidator)
    {
        Context = context;
        Rentals = context.Rentals;
        RentalValidator = rentalValidator;
    }
    
    public RentalValidator RentalValidator { get; }
    public IQueryable<Rental> Rentals { get; }
    public ApplicationDbContext Context { get; set; }
    
    private static string AdminRole = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json").Build().GetSection("Roles")["Admin"];

    public Results<NotFound, Ok<IEnumerable<Rental>>> GetLendings(string token)
    {
        var roles = GetRolesByToken(token);
        var user = GetUserByEmail(GetEmailByToken(token));
        
        var rentals = Rentals.AsQueryable();

        if (!roles.Contains(AdminRole))
        {
            if (user != null)
                rentals = rentals.Where(r => r.UserId == user.Id);
        }
        
        return rentals.ToList().Count > 0 ? TypedResults.Ok(rentals.AsEnumerable()) : TypedResults.NotFound();
    }
    
    public Results<NotFound, ForbidHttpResult, Ok<Rental>> GetLendingById(int id, string token)
    {
        var roles = GetRolesByToken(token);
        var currentUser = GetUserByEmail(GetEmailByToken(token));
        
        var rental = Rentals.FirstOrDefault(r => r.Id == id);

        if (rental == null)
            return TypedResults.NotFound();
        
        if(!roles.Contains(AdminRole) && currentUser != null  && currentUser.Id != rental.UserId)
            return TypedResults.Forbid();

        return TypedResults.Ok(rental);
    }
    
    public Results<NotFound, ForbidHttpResult, Ok<IEnumerable<Rental>>> GetOverdueLendings(int? userId, string token)
    {
        var roles = GetRolesByToken(token);
        var user = GetUserByEmail(GetEmailByToken(token));
        
        var overdueRentals = Rentals.Where(r => r.Until < DateTime.UtcNow && r.ReturnedOn == null).AsQueryable();
       
        if (userId.HasValue)
            overdueRentals = overdueRentals.Where(r => r.UserId == userId);

        if (!roles.Contains(AdminRole))
        {
            if(userId.HasValue && user != null && user.Id != userId)
                return TypedResults.Forbid();
            
            if (user != null)
                overdueRentals = overdueRentals.Where(r => r.UserId == user.Id);
        }
        
        return overdueRentals.ToList().Count > 0 ? TypedResults.Ok(overdueRentals.AsEnumerable()) : TypedResults.NotFound();
    }
    
    public async Task<Results<BadRequest, ForbidHttpResult, Created>> CreateLending(RentalDto rental, string token)
    {
        try
        {
            if (rental.ThingId == null || rental.UserId == null || rental.LendingDurationDays == null)
                return TypedResults.BadRequest();
         
            var roles = GetRolesByToken(token);
            var currentUser = GetUserByEmail(GetEmailByToken(token));
        
            if(!roles.Contains(AdminRole) && currentUser != null && currentUser.Id != rental.UserId)
                return TypedResults.Forbid();
            
            var thing = Context.Things.FirstOrDefault(t => t.Id == rental.ThingId);
            var user = Context.Users.FirstOrDefault(u => u.Id == rental.UserId);
            
            if (thing == null || user == null)
                return TypedResults.BadRequest();

            var newRental = new Rental
            {
                From = DateTime.UtcNow,
                Until = DateTime.UtcNow.AddDays(rental.LendingDurationDays.Value).ToUniversalTime(),
                Thing = thing,
                ThingId = thing.Id,
                User = user,
                UserId = user.Id
            };
            
            if(newRental.Thing.ThingDetails != null)
                await RentalValidator.ValidateAndThrowAsync(newRental);
        
            await Context.Rentals.AddAsync(newRental);
            await Context.SaveChangesAsync();
            return TypedResults.Created();
        }
        catch (ValidationException e)
        {
            return TypedResults.BadRequest();
        }
    }
    
    public async Task<Results<BadRequest, ForbidHttpResult, NotFound, Ok>> UpdateLending(RentalDataDto rental, string token)
    {
        try
        {
            if (rental == null)
                return TypedResults.BadRequest();
            
            if(!(rental.LendingId != null && 
                  (rental.UserId != null || rental.ThingId != null || rental.HandInDate != null || 
                   rental.LendingDurationDays != null)))
            return TypedResults.BadRequest();
            
            Rental currentRental = Rentals
                .Include(r => r.User)
                .Include(r => r.Thing)
                .ThenInclude(t => t.ThingDetails)
                .FirstOrDefault(r => r.Id == rental.LendingId);
            if (currentRental == null)
                return TypedResults.NotFound();
            
            var roles = GetRolesByToken(token);
            var currentUser = GetUserByEmail(GetEmailByToken(token));
        
            if(!roles.Contains(AdminRole) && currentUser != null && currentUser.Id != currentRental.UserId)
                return TypedResults.Forbid();
            
            if (rental.UserId != null)
            {
                var user = Context.Users.FirstOrDefault(u => u.Id == rental.UserId);
                if (user == null)
                    return TypedResults.BadRequest();
                
                currentRental.UserId = rental.UserId.Value;
                currentRental.User = user;
            }
            if (rental.ThingId != null)
            {
                var thing = Context.Things.FirstOrDefault(t => t.Id == rental.ThingId);
                if (thing == null)
                    return TypedResults.BadRequest();
                
                currentRental.ThingId = rental.ThingId.Value;
                currentRental.Thing = thing;
            }
            if (rental.HandInDate != null)
                currentRental.ReturnedOn = rental.HandInDate;
            if (rental.LendingDurationDays != null)
                currentRental.Until = currentRental.From.AddDays(rental.LendingDurationDays.Value);
            
            if(currentRental.Thing.ThingDetails != null)
                await RentalValidator.ValidateAndThrowAsync(currentRental);
            
            Context.Rentals.Update(currentRental);
            await Context.SaveChangesAsync();
            return TypedResults.Ok();
        }
        catch (ValidationException e)
        {
            return TypedResults.BadRequest();
        }
    }
    
    public Results<NotFound, Ok<IEnumerable<Rental>>> GetLendingsByThingsShortName(string shortName, string token)
    {
        var roles = GetRolesByToken(token);
        var user = GetUserByEmail(GetEmailByToken(token));
        
        var rentals = Rentals.Where(r => r.Thing.ShortName == shortName).AsQueryable();

        if (!roles.Contains(AdminRole))
        {
            if (user != null)
                rentals = rentals.Where(r => r.UserId == user.Id);
        }
        
        return rentals.ToList().Count > 0 ? TypedResults.Ok(rentals.AsEnumerable()) : TypedResults.NotFound();
    }
    
    public Results<NotFound, BadRequest, Ok<IEnumerable<Rental>>> GetLendingsByUser(int userId)
    {
        var user = Context.Users.FirstOrDefault(r => r.Id == userId);
        
        if (user == null)
            return TypedResults.BadRequest();
        
        var rentals = Rentals.Where(r => r.UserId == userId).ToList();
        
        return rentals.Count > 0 ? TypedResults.Ok(rentals.AsEnumerable()) : TypedResults.NotFound();
    }
    
    public Results<NotFound, BadRequest, Ok<IEnumerable<Rental>>> GetLendingsByThing(int thingId)
    {
        var thing = Context.Things.FirstOrDefault(r => r.Id == thingId);
        
        if (thing == null)
            return TypedResults.BadRequest();
        
        var rentals = Rentals.Where(r => r.ThingId == thingId).ToList();
        
        return rentals.Count > 0 ? TypedResults.Ok(rentals.AsEnumerable()) : TypedResults.NotFound();
    }
    
    private List<string> GetRolesByToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        List<string> roles = new List<string>();
        
        foreach (var claim in jsonToken.Claims)
        {
            if (claim.Type.Contains("role"))
                roles.Add(claim.Value);
        }

        return roles;
    }
    
    private string GetEmailByToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        
        return jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    }
    
    private User? GetUserByEmail(string email)
    {
        return Context.Users.FirstOrDefault(u => u.Email == email);
    }
}