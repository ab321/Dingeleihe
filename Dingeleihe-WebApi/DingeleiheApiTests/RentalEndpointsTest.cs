using RestSharp;

namespace DingeleiheApiTests;
using System.Net;
using Core.Entities;
using Core.Entities.DTO;
using Persistence;

public class RentalEndpointsTest
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
        context.Rentals.RemoveRange(context.Rentals);
        context.Users.RemoveRange(context.Users);
        context.Things.RemoveRange(context.Things);
        context.Shelves.RemoveRange(context.Shelves);
        await context.SaveChangesAsync();
    }

    private async Task<List<Rental>> PersistRental()
    {
        using var context = new ApplicationDbContext();

        var shelf = new Shelf()
        {
            Location = "Linz"
        };

        var thing = new Thing()
        {
            Description = "ThingDescription",
            SerialNr = "1234",
            ShortName = "ThingShortName",
            Shelf = shelf
        };
        
        var user1 = new User()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = _adminEmail,
            DateOfBirth = new DateTime(1980, 1, 1).ToUniversalTime()
        };
        
        var user2 = new User()
        {
            FirstName = "Mary",
            LastName = "Doe",
            Email = _userEmail,
            DateOfBirth = new DateTime(1985, 1, 1).ToUniversalTime()
        };
        
        var rental1 = new Rental()
        {
            From = DateTime.UtcNow,
            Until = DateTime.UtcNow.AddDays(5).ToUniversalTime(),
            Thing = thing,
            User = user1
        };
        
        var rental2 = new Rental()
        {
            From = DateTime.UtcNow.AddDays(-5).ToUniversalTime(),
            Until = DateTime.UtcNow.AddDays(-1).ToUniversalTime(),
            Thing = thing,
            User = user2
        };
        
        var rental3 = new Rental()
        {
            From = DateTime.UtcNow.AddDays(-5).ToUniversalTime(),
            Until = DateTime.UtcNow.AddDays(-1).ToUniversalTime(),
            Thing = thing,
            User = user1
        };
        
        var rental4 = new Rental()
        {
            From = DateTime.UtcNow.ToUniversalTime(),
            Until = DateTime.UtcNow.AddDays(5).ToUniversalTime(),
            Thing = thing,
            User = user2
        };
        
        var rentals = new List<Rental> {rental1, rental2, rental3, rental4};
        
        context.Rentals.AddRange(rentals);
        await context.SaveChangesAsync();
        return rentals;
    }
    
    [Fact]
    public async Task GivenUser_WhenGetAllLendings_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserWithRentals_WhenGetAllLendings_ThenReturnOkAndOnlyUsersLendings()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.First(r => r.User.Email == _userEmail).Id, response.Data.First().Id);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUser_WhenGetAllLendings_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserWithRentals_WhenGetAllLendings_ThenReturnOkAndLendings()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/all", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.Count(), response.Data.Count());
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUser_WhenGetLendingById_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/1", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<Rental>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserWithRentals_WhenGetLendingByIdOfItSelf_ThenReturnOkAndUsersLending()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/lendings/{rentals.First(r => r.User.Email == _userEmail).Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<Rental>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.First(r => r.User.Email == _userEmail).Id, response.Data.Id);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserWithRentals_WhenGetLendingByIdOfOther_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/lendings/{rentals.First(r => r.User.Email != _userEmail).Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<Rental>(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUser_WhenGetLendingById_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/1", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<Rental>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserWithRentals_WhenGetLendingById_ThenReturnOkAndLending()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/lendings/{rentals.First().Id}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<Rental>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.First().Id, response.Data.Id);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUser_WhenGetOverdueLendings_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/overdue", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserWithRentals_WhenGetOverdueLendings_ThenReturnOkAndOverdueUsersLendings()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/lendings/overdue", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.Where(r => r.User.Email == _userEmail && r.Until < DateTime.UtcNow).Count(), response.Data.Count());
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserWithRentals_WhenGetOverdueLendingByIdOfOther_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/lendings/overdue/{rentals.First(r => r.User.Email != _userEmail).UserId}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUser_WhenGetOverdueLendings_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/overdue", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserWithRentals_WhenGetOverdueLendings_ThenReturnOkAndLendings()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/lendings/overdue", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.Where(r => r.Until < DateTime.UtcNow).Count(), response.Data.Count());
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserWithRentals_WhenGetOverdueLendingsByUserId_ThenReturnOkAndLendings()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/lendings/overdue/{rentals.First(r => r.User.Email == _userEmail).UserId}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");

        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.Where(r => r.User.Email == _userEmail && r.Until < DateTime.UtcNow).Count(), response.Data.Count());
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUser_WhengGetLendingsByUserId_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("userId", 1);
        
        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUser_WhengGetLendingsByThingId_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("thingId", 1);
        
        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndRentals_WhengGetLendingsByUserId_ThenReturnOkWithRentals()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("userId", rentals.First().UserId);
        
        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.Where(r => r.UserId == rentals.First().UserId).Count(), response.Data.Count());
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndRentals_WhenPostLendingWithoutUsersCorrectId_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddBody(new RentalDto()
        {
            LendingDurationDays = 3,
            ThingId = rentals.First().ThingId,
            UserId = rentals.First(r => r.User.Email != _userEmail).UserId
        });
        
        var response = await client.ExecuteAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndRentals_WhenPostLendingWithUsersCorrectId_ThenReturnCreated()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddBody(new RentalDto()
        {
            LendingDurationDays = 3,
            ThingId = rentals.First().ThingId,
            UserId = rentals.First(r => r.User.Email == _userEmail).UserId
        });
        
        var response = await client.ExecuteAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndRentals_WhenPostLending_ThenReturnCreated()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddBody(new RentalDto()
        {
            LendingDurationDays = 3,
            ThingId = rentals.First().ThingId,
            UserId = rentals.First().UserId
        });
        
        var response = await client.ExecuteAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndRentals_WhenUpdateLendingWithoutUsersCorrectId_ThenReturnForbidden()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings", Method.Put);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddBody(new RentalDataDto()
        {
            LendingId = rentals.First(r => r.User.Email != _userEmail).Id,
            LendingDurationDays = 3
        });
        
        var response = await client.ExecuteAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndRentals_WhenUpdateLendingWithUsersCorrectId_ThenReturnOk()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings", Method.Put);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddBody(new RentalDataDto()
        {
            LendingId = rentals.First(r => r.User.Email == _userEmail).Id,
            LendingDurationDays = 3
        });
        
        var response = await client.ExecuteAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndRentals_WhenUpdateLending_ThenReturnOk()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings", Method.Put);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddBody(new RentalDataDto()
        {
            LendingId = rentals.First().Id,
            LendingDurationDays = 3
        });
        
        var response = await client.ExecuteAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUser_WhenGetLendingsByThingsShortName_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/thing/shortName", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");
        
        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenUserAndRentals_WhenGetLendingsByThingsShortName_ThenReturnOkWithUsersLendings()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_userEmail, _userPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/lendings/thing/{rentals.First().Thing.ShortName}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");
        
        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.Where(r => r.Thing.ShortName == rentals.First().Thing.ShortName && r.User.Email == _userEmail).Count(), response.Data.Count());
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUser_WhenGetLendingsByThingsShortName_ThenReturnNotFound()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var client = new RestClient(_baseUrl);
        var request = new RestRequest("/lendings/thing/shortName", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");
        
        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ClearDatabase();
    }
    
    [Fact]
    public async Task GivenAdminUserAndRentals_WhenGetLendingsByThingsShortName_ThenReturnOkWithLendings()
    {
        await ClearDatabase();
        var token = await GetTokenAsync(_adminEmail, _adminPassword);
        var rentals = await PersistRental();
        var client = new RestClient(_baseUrl);
        var request = new RestRequest($"/lendings/thing/{rentals.First().Thing.ShortName}", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");
        
        var response = await client.ExecuteAsync<IEnumerable<Rental>>(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(rentals.Where(r => r.Thing.ShortName == rentals.First().Thing.ShortName).Count(), response.Data.Count());
        await ClearDatabase();
    }
}