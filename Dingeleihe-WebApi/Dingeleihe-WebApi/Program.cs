
using Core.Entities;
using Core.Entities.DTO;
using Core.Entities.Validation;
using Dingeleihe_WebApi;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;

var builder = WebApplication.CreateBuilder(args);


//Setup Dependency Injection
builder.Services.AddDbContext<AppIdentityDbContext>();
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ThingController>();
builder.Services.AddScoped<LibraryCustomerController>();
builder.Services.AddScoped<UserValidator>();
builder.Services.AddScoped<RentalValidator>();
builder.Services.AddScoped<LendingController>();
builder.Services.AddScoped<SecurityController>();

SecurityController.SetupSecurity(builder);

var app = builder.Build();

//use Auth/Author
app.UseAuthentication();
app.UseAuthorization();

//setup initial admin user
SecurityController.AddInitialUsers(app.Services.CreateScope());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", 
    () => "Hello World!");


//things
app.MapGet("/things/all", 
        (ThingController thingCtrl) => thingCtrl.GetThings())
    .RequireAuthorization("adminpolicy");

app.MapGet("/things/{id:int}", 
        [Authorize] (int id, ThingController thingCtrl) => thingCtrl.GetThingById(id))
    .RequireAuthorization("adminpolicy");

app.MapDelete("/things/{id}", 
        [Authorize] async (int id, ThingController thingCtrl) => await thingCtrl.DeleteThingById(id))
    .RequireAuthorization("adminpolicy");

app.MapGet("/things/{shortName}",
        (string shortName, ThingController thingCtrl) => thingCtrl.GetThingsByShortName(shortName))
    .RequireAuthorization("adminpolicy");
    
app.MapPost("/things",
        [Authorize] async ([FromBody] ThingDto thing, ThingController thingCtrl) => await thingCtrl.CreateThing(thing))
    .RequireAuthorization("adminpolicy");

app.MapPost("/shelves",
        [Authorize] async ([FromBody] Shelf shelf, ThingController thingCtrl) => await thingCtrl.CreateShelf(shelf))
    .RequireAuthorization("adminpolicy");

app.MapPatch("/things",
        [Authorize] async ([FromBody] ThingDataDto thing, ThingController thingCtrl) => await thingCtrl.UpdateThing(thing))
    .RequireAuthorization("adminpolicy");

app.MapGet("/things/image/{thingId}",
        [Authorize] (int thingId, ThingController thingCtrl) => thingCtrl.GetImageById(thingId))
    .RequireAuthorization("userpolicy");

app.MapPost("/things/image",
        [Authorize] async ([FromBody] ImageDto image, ThingController thingCtrl) => await thingCtrl.CreateImage(image))
    .RequireAuthorization("adminpolicy");

app.MapDelete("/things/image",
        [Authorize] async ([FromBody] ImageDto image, ThingController thingCtrl) => await thingCtrl.DeleteImage(image.ThingId))
    .RequireAuthorization("adminpolicy");

//libraryCustomers/users
app.MapGet("/libraryCustomers/all",
        [Authorize] (LibraryCustomerController libCustCtrl) => libCustCtrl.GetLibraryCustomers())
    .RequireAuthorization("adminpolicy");

app.MapGet("/libraryCustomers/byId/{id}",
        [Authorize] (int id, LibraryCustomerController libCustCtrl) =>
        {
            HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            var token = httpContextAccessor.HttpContext.Request.Headers.Authorization
                .ToString().Replace("Bearer ", "").Trim();
        
            return libCustCtrl.GetLibraryCustomerById(id, token);
        })
    .RequireAuthorization("userpolicy");

app.MapGet("/libraryCustomers/byEmail/{email}",
        [Authorize] (string email, LibraryCustomerController libCustCtrl) => libCustCtrl.GetLibraryCustomerByEmail(email))
    .RequireAuthorization("adminpolicy");



//lendings
app.MapGet("/lendings/all",
    [Authorize] (LendingController lendingCtrl) =>
    {
        HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        var token = httpContextAccessor.HttpContext.Request.Headers.Authorization
            .ToString().Replace("Bearer ", "").Trim();
        
        return lendingCtrl.GetLendings(token);
    })
    .RequireAuthorization("userpolicy");

app.MapGet("/lendings/{id}",
    [Authorize] (int id, LendingController lendingCtrl) =>
    {
        HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        var token = httpContextAccessor.HttpContext.Request.Headers.Authorization
            .ToString().Replace("Bearer ", "").Trim();

        return lendingCtrl.GetLendingById(id, token);
    })
    .RequireAuthorization("userpolicy");

app.MapGet("/lendings/overdue/{userId?}",
    [Authorize] (int? userId, LendingController lendingCtrl) =>
    {
        HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        var token = httpContextAccessor.HttpContext.Request.Headers.Authorization
            .ToString().Replace("Bearer ", "").Trim();

        return lendingCtrl.GetOverdueLendings(userId, token);
    })
    .RequireAuthorization("userpolicy");

app.MapGet("/lendings",
    [Authorize] (int? userId, int? thingId, LendingController lendingCtrl) =>
    {
        if (userId != null || userId == 0)
            return lendingCtrl.GetLendingsByUser(userId.Value);
        if (thingId != null || thingId == 0)
            return lendingCtrl.GetLendingsByThing(thingId.Value);

        return TypedResults.BadRequest();
    })
    .RequireAuthorization("adminpolicy");

app.MapPost("/lendings",
    [Authorize] async ([FromBody] RentalDto rental, LendingController lendingCtrl) =>
    {
        HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        var token = httpContextAccessor.HttpContext.Request.Headers.Authorization
            .ToString().Replace("Bearer ", "").Trim();
        
        return await lendingCtrl.CreateLending(rental, token);
    })
    .RequireAuthorization("userpolicy");

app.MapPut("/lendings",
    [Authorize] async ([FromBody] RentalDataDto rental, LendingController lendingCtrl) =>
    {
        HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        var token = httpContextAccessor.HttpContext.Request.Headers.Authorization
            .ToString().Replace("Bearer ", "").Trim();

        return await lendingCtrl.UpdateLending(rental, token);
    })
    .RequireAuthorization("userpolicy");

app.MapGet("/lendings/thing/{shortName}",
    [Authorize] (string shortName, LendingController lendingCtrl) =>
    {
        HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        var token = httpContextAccessor.HttpContext.Request.Headers.Authorization
            .ToString().Replace("Bearer ", "").Trim();
        
        return lendingCtrl.GetLendingsByThingsShortName(shortName, token);
    })
    .RequireAuthorization("userpolicy");


//security
app.MapPost("/security/login",
    async (SecurityController secCtrl, [FromBody] WebApiUser user) => await secCtrl.Login(user));

app.MapPost("/security/shouldBeObfuscated", 
        async (SecurityController secCtrl, ApplicationDbContext context, [FromBody] WebApiUser user, string firstName, string lastName, DateTime? dob) =>
        {
            Results<Ok, BadRequest> result = TypedResults.BadRequest();
            var addUser = await secCtrl.CreateUser(user);
            UserValidator userValidator = new UserValidator();

            if (addUser.Result is Ok)
            {
                var libraryCustomer = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = user.NormalizedEmail
                };

                if (dob != null)
                    libraryCustomer.DateOfBirth = DateTime.SpecifyKind(dob.Value, DateTimeKind.Utc);

                try
                {
                    userValidator.ValidateAndThrow(libraryCustomer);
                    
                    context.Add(libraryCustomer);
                    context.SaveChanges();
                    result = TypedResults.Ok();
                }
                catch (Exception e)
                {
                    await secCtrl.DeleteUser(user);
                }
            }

            return result;
        });

app.MapDelete("/security/shouldBeObfuscated",
    [Authorize] async (SecurityController secCtrl, ApplicationDbContext context, [FromBody] WebApiUser user) =>
    {
        Results<NoContent, BadRequest> result = TypedResults.BadRequest();
        var deleteUser = await secCtrl.DeleteUser(user);
        
        if (deleteUser.Result is NoContent)
        {
            var libraryCustomer = context.Users.FirstOrDefault(u => u.Email == user.NormalizedEmail);

            if (libraryCustomer is not null)
            {
                context.Remove(libraryCustomer);

                try
                {
                    context.SaveChanges();
                    result = TypedResults.NoContent();
                }
                catch (Exception e)
                {
                    await secCtrl.CreateUser(user);
                }
            }
        }
        
        return result;
    })
    .RequireAuthorization("adminpolicy");

app.MapPut("/security/shouldBeObfuscated",
        [Authorize] async (SecurityController secCtrl, [FromBody] WebApiUserDto user) => await secCtrl.UpdateUser(user))
    .RequireAuthorization("adminpolicy");

app.MapPost("/security/user/makeadmin",
        [Authorize] async (SecurityController secCtrl, [FromBody] WebApiUser user) => await secCtrl.GrantAdmin(user))
    .RequireAuthorization("adminpolicy");

app.MapDelete("/security/user/makeadmin",
    [Authorize] async (SecurityController secCtrl, [FromBody] WebApiUser user) => await secCtrl.RevokeAdmin(user))
    .RequireAuthorization("adminpolicy");

app.Run();    