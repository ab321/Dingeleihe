using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Entities;
using Core.Entities.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Dingeleihe_WebApi;

public class SecurityController
{
    private static string Issuer;
    private static string Audience;
    private static SymmetricSecurityKey SecurityKey;
    private readonly UserManager<IdentityUser> userMgr;

    private static string AdminRole = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json").Build().GetSection("Roles")["Admin"];
    private static string UserRole = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json").Build().GetSection("Roles")["User"];
    
    private static string AdminEmail = "johndoe";
    private static string UserEmail = "marydoe";

    public SecurityController(UserManager<IdentityUser> userMgr)
    {
        this.userMgr = userMgr;
    }

    public static void SetupSecurity(WebApplicationBuilder builder)
    {
        Issuer = builder.Configuration["Jwt:Issuer"];
        Audience = builder.Configuration["Jwt:Audience"];
        SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));

        builder.Services.AddScoped<SecurityController>();
        builder.Services.AddEndpointsApiExplorer();

        //Configure for Authorization
        //http://www.binaryintellect.net/articles/082e1b54-86ec-495a-86fb-be260830947c.aspx
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer", //This must be prefixed before the Token: Bearer <Token>
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JSON Web Token based security"
        };

        var securityReq = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        };

        builder.Services.AddSwaggerGen(o =>
        {
            o.AddSecurityDefinition("Bearer", securityScheme);
            o.AddSecurityRequirement(securityReq);
        });

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, 
                    ClockSkew = TimeSpan.Zero, 
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    IssuerSigningKey = SecurityKey
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("adminpolicy", policy =>
                policy
                    .RequireRole(AdminRole)
                    .RequireClaim("scope", Audience))
            .AddPolicy("userpolicy", policy =>
                policy
                    .RequireRole(UserRole)
                    .RequireClaim("scope", Audience));
    }

    public static void AddInitialUsers(IServiceScope serviceScope)
    {
        var idDb = serviceScope.ServiceProvider.GetService<UserManager<IdentityUser>>();
        var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

        if (roleManager is not null)
        {
            var adminRole = new IdentityRole(AdminRole);
            roleManager.CreateAsync(adminRole).GetAwaiter().GetResult();
            
            var userRole = new IdentityRole(UserRole);
            roleManager.CreateAsync(userRole).GetAwaiter().GetResult();
        }

        {
            var identityAdminUser = new IdentityUser
            {
                UserName = AdminEmail.ToUpper(),
                Email = AdminEmail,
                NormalizedEmail = AdminEmail.ToUpper()
            };

            var result = idDb.CreateAsync(identityAdminUser, "Wuxi123$%&").GetAwaiter().GetResult();
            idDb.AddToRolesAsync(identityAdminUser, new []{AdminRole, UserRole}).GetAwaiter().GetResult();
        }

        {
            var identityUser = new IdentityUser
            {
                UserName = UserEmail.ToUpper(),
                Email = UserEmail,
                NormalizedEmail = UserEmail.ToUpper()
            };

            var result = idDb.CreateAsync(identityUser, "Kolt123$%&").GetAwaiter().GetResult();
            idDb.AddToRoleAsync(identityUser, UserRole).GetAwaiter().GetResult();
        }
    }

    /*public async Task<Results<Ok, BadRequest>> CreateUser(WebApiUser user)
    {
        var identityUser = new IdentityUser
        {
            UserName = user.NormalizedEmail.ToUpper(),
            Email = user.NormalizedEmail,
            NormalizedEmail = user.NormalizedEmail.ToUpper()
        };

        await userMgr.AddToRoleAsync(identityUser, UserRole);

        var result = await userMgr.CreateAsync(identityUser, user.Password);

        return result.Succeeded ? TypedResults.Ok() : TypedResults.BadRequest();
    }*/
    
    public async Task<Results<Ok, BadRequest>> CreateUser(WebApiUser user)
    {
        var identityUser = new IdentityUser
        {
            UserName = user.NormalizedEmail.ToUpper(),
            Email = user.NormalizedEmail,
            NormalizedEmail = user.NormalizedEmail.ToUpper()
        };

        var result = await userMgr.CreateAsync(identityUser, user.Password);

        if (result.Succeeded)
        {
            var roleResult = await userMgr.AddToRoleAsync(identityUser, UserRole);
            if (!roleResult.Succeeded)
            {
                // Handle role assignment failure
                return TypedResults.BadRequest();
            }
        }
        else
        {
            // Handle user creation failure
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok();
    }
    
    public async Task<Results<Ok, BadRequest>> UpdateUser(WebApiUserDto user)
    {
        if (user.NormalizedEmail == AdminEmail)
            return TypedResults.BadRequest();
        
        var identityUsr = await userMgr.FindByEmailAsync(user.NormalizedEmail.ToUpper());
        if (identityUsr is not null)
        {
            var result = await userMgr.ChangePasswordAsync(identityUsr, user.CurrentPassword, user.NewPassword);
            return result.Succeeded ? TypedResults.Ok() : TypedResults.BadRequest();
        }
        
        return TypedResults.BadRequest();
    }
    
    public async Task<Results<NoContent, BadRequest>> DeleteUser(WebApiUser user)
    {
        if (user.NormalizedEmail == AdminEmail)
            return TypedResults.BadRequest();
        
        var result = IdentityResult.Failed();
        var identityUsr = await userMgr.FindByEmailAsync(user.NormalizedEmail.ToUpper());
        var pwdCheck = await userMgr.CheckPasswordAsync(identityUsr, user.Password);
        if (identityUsr is not null && pwdCheck) result = await userMgr.DeleteAsync(identityUsr);
        return result.Succeeded ? TypedResults.NoContent() : TypedResults.BadRequest();
    }

    public async Task<Results<Ok<string>, UnauthorizedHttpResult>> Login(WebApiUser user)
    {
        var identityUsr = await userMgr.FindByEmailAsync(user.NormalizedEmail.ToUpper());

        if (await userMgr.CheckPasswordAsync(identityUsr, user.Password))
        {
            var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.NormalizedEmail), 
                new("scope", Audience),
                new("UserId", identityUsr.Id)
            };
            foreach (var role in await userMgr.GetRolesAsync(identityUsr)) 
                claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(Issuer, Audience, signingCredentials: credentials,
                expires: DateTime.UtcNow.AddMinutes(30),
                claims: claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var stringToken = tokenHandler.WriteToken(token);

            return TypedResults.Ok(stringToken);
        }

        return TypedResults.Unauthorized();
    }
    
    public async Task<Results<Ok, BadRequest>> GrantAdmin(WebApiUser user)
    {
        var identityUsr = await userMgr.FindByEmailAsync(user.NormalizedEmail.ToUpper());
        if (identityUsr is not null)
        {
            var r = await userMgr.CheckPasswordAsync(identityUsr, user.Password);
            
            if(r){
                var result = await userMgr.AddToRoleAsync(identityUsr, AdminRole);
                return result.Succeeded ? TypedResults.Ok() : TypedResults.BadRequest();
            }
        }
        
        return TypedResults.BadRequest();
    }
    
    public async Task<Results<Ok, BadRequest>> RevokeAdmin(WebApiUser user)
    {
        var identityUsr = await userMgr.FindByEmailAsync(user.NormalizedEmail.ToUpper());
        if (identityUsr is not null)
        {
            var r = await userMgr.CheckPasswordAsync(identityUsr, user.Password);
            
            if(r){
                var result = await userMgr.RemoveFromRoleAsync(identityUsr, AdminRole);
                return result.Succeeded ? TypedResults.Ok() : TypedResults.BadRequest();
            }
        }
        
        return TypedResults.BadRequest();
    }
}
