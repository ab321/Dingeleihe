using System.Diagnostics;
using Core.Entities;
using Core.Entities.Validation;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace DingeleiheTests;

[TestClass]
public class ApplicationDbContextModelTests
{
    /*[TestMethod]
    public void ThingDetailsImageDate_AccessByProxyComparedToInclude_IncludeIsFaster() // Wies im unterricht gezeigt wurde
    {
        using var context = new ApplicationDbContext();
        using var transaction = context.Database.BeginTransaction();
        
        var random = new Random();
        var data = new byte[1024 * 1024 * 10];
        random.NextBytes(data);
        
        
        var thing = new Thing
        {
            ShortName = "TestThing",
            Description = "TestDescription",
            SerialNr = "12345"
        };

        context.Add(thing);
        context.SaveChanges();

        var thingDetails = new ThingDetails
        {
            Thing = thing,
            ThingId = thing.Id,
            Image = new ImageData { Data = data },
            AgeRestriction = 18
        };

        context.Add(thingDetails);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        
        var queriedThingDetails = context.ThingDetails.Single(td => td.Id == thingDetails.Id);
        
        var watch = new Stopwatch();
        watch.Start();
        var imageDataFromQueriedThingDetails = queriedThingDetails.Image;
        watch.Stop();
        var timeForProxy = watch.ElapsedMilliseconds;
        
        var queriedThingDetailsWithInclude = context.ThingDetails
            .Include(td => td.Image)
            .Single(td => td.Id == thingDetails.Id);
        
        watch.Restart();
        var imageDataFromQueriedThingDetailsWithImage = queriedThingDetails.Image;
        watch.Stop();
        
        Assert.IsNotNull(imageDataFromQueriedThingDetailsWithImage);
    }*/
    
    [TestMethod]
    public void Given_ApplicationDbContext_When_CreateContext_ThenIsNotNull()
    {
        using (var context = new ApplicationDbContext())
        {
            Assert.IsNotNull(context);
        }
    }
    
    [TestMethod]
    public void GivenTooYoungUser_WhenSaveUser_ThenCannotValidate() 
    {
        var userValidator = new UserValidator();

        var user = new User
        {
            FirstName = "Abdii",
            LastName = "Aldosoki",
            Email = "abdul@gmx.com",
            DateOfBirth = new DateTime(2020, 1, 1, 1, 1, 1, DateTimeKind.Utc)
        };
        
        var result = userValidator.Validate(user);
        
        Assert.AreEqual("Der Benutzer muss mindestens 15 Jahre alt sein.", result.Errors.ElementAt(0).ErrorMessage);
    }
    
    [TestMethod]
    public async Task Given_ValidUser_When_SaveUser_Then_SaveAsync()
    {
        using var context = new ApplicationDbContext();
        using var transaction = context.Database.BeginTransaction();
        
        var dob = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var user = new User
        {
            FirstName = "Abdullah"
            ,LastName = "Aldesoky"
            ,Email = "Abdul@gmx.com"
            ,DateOfBirth = dob
        };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        Assert.AreEqual(user.FirstName, context.Users.ElementAt(0).FirstName);
        Assert.AreEqual(user.LastName, context.Users.ElementAt(0).LastName);
        Assert.AreEqual(user.Email, context.Users.ElementAt(0).Email);
        Assert.AreEqual(user.DateOfBirth, dob);
        
        await transaction.RollbackAsync();
    }
    
    [TestMethod]
    public async Task GivenValidThing_WhenSaveThing_ThenCanSaveAsync()
    {
        using var context = new ApplicationDbContext();
        using var transaction = context.Database.BeginTransaction();
        
        var shelf = new Shelf()
        {
            Location = "A4"
        };
        
        var thing = new Thing
        {
            ShortName = "Thing",
            Description = "This is a dangerous thing",
            SerialNr = "937t4b",
            Shelf = shelf
        };
        
        var thingDetails = new ThingDetails
        {
            Thing = thing,
            AgeRestriction = 18
        };

        await context.Things.AddAsync(thing);
        await context.ThingDetails.AddAsync(thingDetails);
        await context.SaveChangesAsync();
 
        Assert.AreEqual(thing.ShortName, context.Things.ElementAt(0).ShortName);
        Assert.AreEqual(thing.Description, context.Things.ElementAt(0).Description);
        Assert.AreEqual(thing.SerialNr, context.Things.ElementAt(0).SerialNr);

        Assert.AreEqual(thingDetails.ThingId, context.ThingDetails.ElementAt(0).ThingId);   
        Assert.AreEqual(thingDetails.AgeRestriction, context.ThingDetails.ElementAt(0).AgeRestriction);
        
        await transaction.RollbackAsync();
    }
    
    [TestMethod]
    [ExpectedException(typeof(DbUpdateException))]
    public async Task GivenThreeUsersWithSameEmail_WhenSaveUsers_ThenCannotSaveAsync()
    {
        using var context = new ApplicationDbContext();
        using var transaction = context.Database.BeginTransaction();
        
        var dob1 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dob2 = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dob3 = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var user1 = new User
        {
            FirstName = "Abdullah"
            ,LastName = "Aldesoky"
            ,Email = "Abdul@gmx.com"
            ,DateOfBirth = dob1
        };
        
        var user2 = new User
        {
            FirstName = "Abdullah"
            ,LastName = "Aldesoky"
            ,Email = "Abdul@gmx.com"
            ,DateOfBirth = dob2
        };
        
        var user3 = new User
        {
            FirstName = "Abdullah"
            ,LastName = "Aldesoky"
            ,Email = "Abdul@gmx.com"
            ,DateOfBirth = dob2
        };
        
        await context.Users.AddAsync(user1);
        await context.Users.AddAsync(user2);
        await context.Users.AddAsync(user3);
        await context.SaveChangesAsync();
        
        await transaction.RollbackAsync();
    }
    
    [TestMethod]
    [ExpectedException(typeof(DbUpdateException))]
    public async Task GivenTwoThingsWithSameSerialNr_WhenSaveThings_ThenCannotSaveAsync()
    {
        using var context = new ApplicationDbContext();
        using var transaction = context.Database.BeginTransaction();
        
        var dob1 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dob2 = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dob3 = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var thing1 = new Thing
        {
            ShortName = "Thing1",
            Description = "This is a thing2",
            SerialNr = "937t4b"
        };
        
        var thing2 = new Thing
        {
            ShortName = "Thing",
            Description = "This is a thing",
            SerialNr = "937t4b"
        };


        await context.Things.AddAsync(thing1);
        await context.Things.AddAsync(thing2);  
        await context.SaveChangesAsync();
        
        await transaction.RollbackAsync();
    }
    
    [TestMethod]
    public async Task GivenValidRental_WhenSaveRental_ThenSaveAsync()
    {
        using var context = new ApplicationDbContext();
        using var transaction = context.Database.BeginTransaction();
        
        var shelf = new Shelf()
        {
            Location = "Irgendwo"
        };

        var user = new User
        {
            FirstName = "Abdii",
            LastName = "Aldosoki",
            Email = "abdul@gmx.com",
            DateOfBirth = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc)
        };

        var thing = new Thing
        {
            ShortName = "Thing",
            Description = "This is a thing",
            SerialNr = "23f46k",
            Shelf = shelf
        };

        var rental = new Rental
        {
            From = new DateTime(2022, 3, 20, 0, 0, 0, DateTimeKind.Utc),
            Until = new DateTime(2022, 4, 4, 0, 0, 0, DateTimeKind.Utc),
            User = user,
            Thing = thing
        };
        
        await context.Users.AddAsync(user);
        await context.Shelves.AddAsync(shelf);
        await context.Things.AddAsync(thing);
        await context.Rentals.AddAsync(rental);
        await context.SaveChangesAsync();
        
        await transaction.RollbackAsync();
    }
    
    [TestMethod]
    public async Task GivenValidShelf_WhenSaveShelf_ThenSaveAsync()
    {
        using var context = new ApplicationDbContext();
        using var transaction = context.Database.BeginTransaction();
        
        var shelf = new Shelf
        {
            Location = "Irgendwo in niergendwo",
            Things = new List<Thing>()
        };
        
        var thingOne = new Thing
        {
            ShortName = "Thing1",
            Description = "This is a thing",
            SerialNr = "984rt6i" 
        };
        
        var thingTwo = new Thing
        {
            ShortName = "Thing2",
            Description = "This is a thing",
            SerialNr = "393reifb"
        };
        
        await context.Things.AddAsync(thingOne);
        await context.Things.AddAsync(thingTwo);

        shelf.Things.Add(thingOne);
        shelf.Things.Add(thingTwo);
        await context.Shelves.AddAsync(shelf);
        await context.SaveChangesAsync();
        
        Assert.AreEqual(shelf.Location, context.Shelves.ElementAt(0).Location);
        Assert.AreEqual(shelf.Things.Count, context.Shelves.ElementAt(0).Things.Count());
        
        await transaction.RollbackAsync();
    }
    
    [TestMethod]
    public void GivenRentalWithTooYoungUser_WhenSaveRental_ThenCannotValidate() 
    {
        var rentalValidator = new RentalValidator();

        var rental = new Rental
        {
            From = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Until = new DateTime(2021, 1, 4, 0, 0, 0, DateTimeKind.Utc),
            User = new User
            {
                FirstName = "Abdii",
                LastName = "Aldosoki",
                Email = "abdul@gmx.com",
                DateOfBirth = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc)
            },
            Thing = new Thing
            {
                ShortName = "Thing",
                Description = "Very Dangerous Thing",
                SerialNr = "49rzun",
                ThingDetails = new ThingDetails
                {
                    AgeRestriction = 30,
                    ThingId = 5
                }
            }
        };

        var result = rentalValidator.Validate(rental);
        
        Assert.AreEqual(
            "Der Benutzer ist nicht alt genug, um dieses Ding auszuleihen.", 
            result.Errors.ElementAt(0).ErrorMessage
        );
    }
}