using System.Net;
using Core.Entities;
using Core.Entities.DTO;
using Persistence;
using RestSharp;

namespace DingeleiheApiTests;

public class ThingEnpointsTest
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
    
    private async Task<Shelf> PersistShelf()
    {
        using var context = new ApplicationDbContext();
        var shelf = new Shelf()
        {
            Location = "Linz"
        };

        context.Shelves.Add(shelf);
        await context.SaveChangesAsync();

        return shelf;
    }
    
    private async Task ClearDatabase()
    {
        using var context = new ApplicationDbContext();
        context.Things.RemoveRange(context.Things);
        context.Shelves.RemoveRange(context.Shelves);
        await context.SaveChangesAsync();
    }
    
    private async Task<Thing> PersistThing()
    {
        using var context = new ApplicationDbContext();
        var shelf = await PersistShelf();
        var thing = new Thing()
        {
            ShortName = "TestShortName",
            Description = "Test Description",
            SerialNr = "123456",
            ShelfId = shelf.Id, 
            ThingDetails = new ThingDetails()
            {
                AgeRestriction = 12,
                Image = new Image()
                {
                    Data = new byte[] { 0x10, 0x10 }
                }
            }
        };
        
        context.Things.Add(thing);
        await context.SaveChangesAsync();

        return thing;
    }
    
    [Fact]
    public async Task GivenAdminUser_WhenGetAllThings_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndThing_WhenGetAllThings_ThenReturnOk()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndThing_WhenGetAllThings_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }

    [Fact]
    public async Task GivenAdminUser_WhenGetThingById_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/1", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}"); 

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndThing_WhenGetThingById_ThenReturnOk()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/things/{thing.Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}"); 

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndThing_WhenGetThingById_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/things/{thing.Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}"); 

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }

    [Fact]
    public async Task GivenAdminUser_WhenDeleteThingById_ThenReturnBadRequest()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/1", Method.Delete);
        request.AddHeader("Authorization", $"Bearer {token}"); 

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndThing_WhenDeleteThingById_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/things/{thing.Id}", Method.Delete);
        request.AddHeader("Authorization", $"Bearer {token}"); 

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }

    [Fact]
    public async Task GivenAdminUser_WhenGetThingByShortName_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/shortname", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}"); 

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndThing_WhenGetThingByShortName_ThenReturnOk()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/things/{thing.ShortName}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}"); 

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndThing_WhenGetThingByShortName_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/things/{thing.ShortName}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}"); 

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }

    [Fact]
    public async Task GivenAdminUserAndThing_WhenPostThing_ThenReturnCreated()
    {
        await ClearDatabase();
        var shelf = await PersistShelf();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}"); 
        request.AddJsonBody(new ThingDto()
        {
            ShortName = "TestShortName",
            Description = "Test Description",
            SerialNr = "123456",
            ShelfId = shelf.Id,
            AgeRestriction = 14
        });

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndThing_WhenPostThing_ThenReturnForbidden()
    {
        await ClearDatabase();
        var shelf = await PersistShelf();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}"); 
        request.AddJsonBody(new ThingDto()
        {
            ShortName = "TestShortName",
            Description = "Test Description",
            SerialNr = "123456",
            ShelfId = shelf.Id,
            AgeRestriction = 14
        });

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }

    [Fact]
    public async Task GivenAdminUserAndThing_WhenPatchThing_ThenReturnOk()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things", Method.Patch);
        request.AddHeader("Authorization", $"Bearer {token}"); 
        request.AddJsonBody(new ThingDataDto()
        {
            ThingId = thing.Id,
            ShortName = "UpdatedShortName"
        });

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndThing_WhenPatchThing_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things", Method.Patch);
        request.AddHeader("Authorization", $"Bearer {token}"); 
        request.AddJsonBody(new ThingDataDto()
        {
            ThingId = thing.Id,
            ShortName = "UpdatedShortName"
        });

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }

    [Fact]
    public async Task GivenAdminUser_WhenGetImageByThingsId_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/image/1", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndThing_WhenGetImageByThingsId_ThenReturnOk()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/things/image/{thing.Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndThing_WhenGetImageByThingsId_ThenReturnOk()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/things/image/{thing.Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await ClearDatabase();
    }

    [Fact]
    public async Task GivenAdminUser_WhenDeleteImageByThingsId_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/image", Method.Delete);
        request.AddHeader("Authorization", $"Bearer {token}"); 
        request.AddJsonBody(new ImageDto()
        {
            ThingId = 1
        });

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndThing_WhenDeleteImageByThingsId_ThenReturnOk()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/image", Method.Delete);
        request.AddHeader("Authorization", $"Bearer {token}"); 
        request.AddJsonBody(new ImageDto()
        {
            ThingId = thing.Id
        });

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndThing_WhenDeleteImageByThingsId_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/image", Method.Delete);
        request.AddHeader("Authorization", $"Bearer {token}"); 
        request.AddJsonBody(new ImageDto()
        {
            ThingId = thing.Id
        });

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }

    [Fact]
    public async Task GivenAdminUserAndThing_WhenCreateImageForThing_ThenReturnCreated()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/image", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}"); 
        request.AddJsonBody(new ImageDto()
        {
            ThingId = thing.Id,
            Blob = new byte[] { 0x20, 0x20 } 
        });

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndThing_WhenCreateImageForThing_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var thing = await PersistThing();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/things/image", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}"); 
        request.AddJsonBody(new ImageDto()
        {
            ThingId = thing.Id,
            Blob = new byte[] { 0x20, 0x20 } 
        });

        var response = await client.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
}