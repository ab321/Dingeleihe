using RestSharp;

namespace DingeleiheApiTests;
using System.Net;
using Core.Entities;
using Core.Entities.DTO;
using Persistence;

public class UserEndpointsTest
{
    private readonly string _baseUrl = "http://localhost:5555"; 
    private static string _adminEmail = "johndoe";
    private static string _adminPassword = "Wuxi123$%&";
    
    private static string _userEmail = "marydoe";
    private static string _userPassword = "Kolt123$%&";
    
    private async Task<string> GetTokenAsync(string email, string password)
    {
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/security/login", Method.Post);
        request.AddJsonBody(new WebApiUser()
        {
            NormalizedEmail = email,
            Password = password
        });

        var response = await client.ExecuteAsync<string>(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Data != null)
        {
            return response.Data;
        }

        throw new Exception("Failed to retrieve token");
    }
    
    private async Task ClearDatabase()
    {
        using var context = new ApplicationDbContext();
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();
    }
    
    private async Task<List<User>> PersistUsers()
    {
        using var context = new ApplicationDbContext();
        
        var users = new List<User>
        {
            new User()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = _adminEmail,
                DateOfBirth = new DateTime(1980, 1, 1).ToUniversalTime()
            },
            new User()
            {
                FirstName = "Mary",
                LastName = "Doe",
                Email = _userEmail,
                DateOfBirth = new DateTime(1985, 1, 1).ToUniversalTime()
            }
        };
        
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        return users;
    }
    
    [Fact]
    public async Task GivenUser_WhenGetLibraryCustomers_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/librarycustomers/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<User>>(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUser_WhenGetLibraryCustomers_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/librarycustomers/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<User>>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndUsers_WhenGetLibraryCustomers_ThenReturnOkWithUsers()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var users = await PersistUsers();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/librarycustomers/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<User>>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(users.Count, response.Data.Count());
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUser_WhenGetLibraryCustomerByEmail_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/librarycustomers/byEmail/{_adminEmail}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<User>(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUser_WhenGetLibraryCustomerByEmail_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/librarycustomers/byEmail/{_adminEmail}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<User>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndUsers_WhenGetLibraryCustomerByEmail_ThenReturnOkWithUser()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var users = await PersistUsers();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/librarycustomers/byEmail/{_adminEmail}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<User>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(users.First(u => u.Email == _adminEmail).Email, response.Data.Email);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUser_WhenGetLibraryCustomerById_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/librarycustomers/byId/1", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<User>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndUsers_WhenGetLibraryCustomerByIdOfItself_ThenReturnOkWithUser()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var users = await PersistUsers();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/librarycustomers/byId/{users.Last().Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<User>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(users.Last().Email, response.Data.Email);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndUsers_WhenGetLibraryCustomerByIdOfOther_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var users = await PersistUsers();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/librarycustomers/byId/{users.First().Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<User>(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUser_WhenGetLibraryCustomerById_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/librarycustomers/byId/1", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<User>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndUsers_WhenGetLibraryCustomerById_ThenReturnOkWithUser()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var users = await PersistUsers();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/librarycustomers/byId/{users.First().Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<User>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(users.First().Email, response.Data.Email);
        await ClearDatabase();
    }
}