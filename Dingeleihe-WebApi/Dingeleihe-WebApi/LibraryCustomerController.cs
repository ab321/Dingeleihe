using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Entities;
using Core.Entities.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Persistence;

namespace Dingeleihe_WebApi;

public class LibraryCustomerController
{
    public LibraryCustomerController(ApplicationDbContext context, UserValidator userValidator)
    {
        Context = context;
        Users = context.Users;
        UserValidator = userValidator;
    }
    
    public UserValidator UserValidator { get; }
    public IQueryable<User> Users { get; }
    public ApplicationDbContext Context { get; set; }
    
    private static string AdminRole = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json").Build().GetSection("Roles")["Admin"];
    
    public Results<NotFound, Ok<IEnumerable<User>>> GetLibraryCustomers()
    {
        var users = Users.ToList();
        return users.Count > 0 ? TypedResults.Ok(users.AsEnumerable()) : TypedResults.NotFound();
    }
    
    public Results<NotFound, ForbidHttpResult, Ok<User>> GetLibraryCustomerById(int id, string token)
    {
        var roles = GetRolesByToken(token);
        var currentUser = GetUserByEmail(GetEmailByToken(token));
        
        if(!roles.Contains(AdminRole) && currentUser != null && currentUser.Id != id)
            return TypedResults.Forbid();
        
        var user = Users.FirstOrDefault(u => u.Id == id);
        return user != null ? TypedResults.Ok(user) : TypedResults.NotFound();
    }
    
    public Results<NotFound, Ok<User>> GetLibraryCustomerByEmail(string email)
    {
        var user = Users.FirstOrDefault(u => u.Email == email);
        return user != null ? TypedResults.Ok(user) : TypedResults.NotFound();
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