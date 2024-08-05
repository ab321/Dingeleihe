using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Core.Entities;
using Core.Entities.DTO;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Persistence;
using RestSharp;

namespace DingeleiheApiTests;

public class SecurityEndpointsTest
{
    private readonly string _baseUrl = "http://localhost:5555"; 
    private static string _adminEmail = "johndoe";
    private static string _adminPassword = "Wuxi123$%&";
    
    private static string _userEmail = "marydoe";
    private static string _userPassword = "Kolt123$%&";
    
    private string GetEmailByToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        
        return jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    }
    
    
    [Fact]
    public async Task GivenUser_WhenLogin_ThenReturnOkWithCorrectToken()
    {
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/security/login", Method.Post);
        request.AddJsonBody(new WebApiUser()
        {
            NormalizedEmail = _userEmail,
            Password = _userPassword
        });

        var response = await client.ExecuteAsync<string>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(_userEmail, GetEmailByToken(response.Data));
    }
    
    [Fact]
    public async Task GivenAdminUser_WhenLogin_ThenReturnOkWithCorrectToken()
    {
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/security/login", Method.Post);
        request.AddJsonBody(new WebApiUser()
        {
            NormalizedEmail = _adminEmail,
            Password = _adminPassword
        });

        var response = await client.ExecuteAsync<string>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(_adminEmail, GetEmailByToken(response.Data));
    }
    
    [Fact]
    public async Task GivenUnknownnUser_WhenLogin_ThenReturnUnauthorized()
    {
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/security/login", Method.Post);
        request.AddJsonBody(new WebApiUser()
        {
            NormalizedEmail = "unknown",
            Password = "unknwon123$%&"
        });

        var response = await client.ExecuteAsync<string>(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}