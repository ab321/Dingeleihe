using Core.Entities;
using Core.Entities.DTO;
using Dingeleihe_WebApi;
using Microsoft.AspNetCore.Http.HttpResults;
using Persistence;

namespace DingeleiheApiTests;

public class ThingControllerTest
{
    private readonly ThingController target;
    
    public ThingControllerTest()
    {
        target = new ThingController(new ApplicationDbContext());
    }
    
    public async Task ClearDb()
    {
        using var context = new ApplicationDbContext();
        context.Things.RemoveRange(context.Things);
        context.Shelves.RemoveRange(context.Shelves);
        await context.SaveChangesAsync();
    }
    
    
    [Fact]
    public async Task GivenMultipleThings_WhenGetThings_ThenReturnOkWithThings()
    {
        // Prepare
        await ClearDb();
        
        Thing thing1 = new Thing()
        {
            SerialNr = "123",
            ShortName = "T1",
            Description = "Thing1 Description",
            Shelf = new Shelf()
            {
                Location = "Vienna"
            }
        };
        
        Thing thing2 = new Thing()
        {
            SerialNr = "456",
            ShortName = "T2",
            Description = "Thing2 Description",
            Shelf = new Shelf()
            {
                Location = "Linz"
            }
        };
        
        target.Context.Things.Add(thing1);
        target.Context.Things.Add(thing2);
        await target.Context.SaveChangesAsync();
        
        // Arrange & Act
        var result = target.GetThings();
        var ok = result.Result as Ok<IEnumerable<Thing>>;

        // Assert
        Assert.IsType<Ok<IEnumerable<Thing>>>(result.Result);
        Assert.NotNull(ok);
        Assert.Equal(2, ok.Value.Count());
        Assert.Contains(thing1, ok.Value);
        Assert.Contains(thing2, ok.Value);
        await ClearDb();
    }
    
    
    [Fact]
    public async Task GivenMultipleThings_WhenGetThingById_ThenReturnOkWithThing()
    {
        // Prepare
        await ClearDb();
        
        Thing thing = new Thing()
        {
            SerialNr = "123",
            ShortName = "T1",
            Description = "Thing1 Description",
            Shelf = new Shelf()
            {
                Location = "Vienna"
            }
        };
        
        target.Context.Things.Add(thing);
        await target.Context.SaveChangesAsync();
        
        // Arrange & Act
        var result = target.GetThingById(thing.Id);
        var ok = result.Result as Ok<Thing>;
        
        // Assert
        Assert.IsType<Ok<Thing>>(result.Result);
        Assert.NotNull(ok);
        Assert.Equal(thing.ShortName, ok.Value.ShortName);
        await ClearDb();
    }
    
    [Fact]
    public async Task GivenMultipleThings_WhenDeleteThingById_ThenReturnOk()
    {
        // Prepare
        await ClearDb();
        
        Thing thing1 = new Thing()
        {
            SerialNr = "123",
            ShortName = "T1",
            Description = "Thing1 Description",
            Shelf = new Shelf()
            {
                Location = "Vienna"
            }
        };
        
        Thing thing2 = new Thing()
        {
            SerialNr = "456",
            ShortName = "T2",
            Description = "Thing2 Description",
            Shelf = new Shelf()
            {
                Location = "Linz"
            }
        };
        
        target.Context.Things.Add(thing1);
        target.Context.Things.Add(thing2);
        await target.Context.SaveChangesAsync();
        
        // Arrange & Act
        var result = await target.DeleteThingById(thing1.Id);
        
        // Assert
        Assert.IsType<Ok>(result.Result);
        await ClearDb();
    }
    
    
    [Fact]
    public async Task Given_WhenGetThingsByShortName_ThenReturnNotFound()
    {
        await ClearDb();
        
        // Arrange & Act
        var result = target.GetThingsByShortName("shortName");
        
        // Assert
        Assert.IsType<NotFound>(result.Result);
        await ClearDb();
    }
    
    [Fact]
    public async Task GivenMultipleThings_WhenGetThingsByShortName_ThenReturnOkWithThings()
    {
        // Prepare
        await ClearDb();
        
        Thing thing1 = new Thing()
        {
            SerialNr = "123",
            ShortName = "T1",
            Description = "Thing1 Description",
            Shelf = new Shelf()
            {
                Location = "Vienna"
            }
        };
        
        Thing thing2 = new Thing()
        {
            SerialNr = "456",
            ShortName = "T2",
            Description = "Thing2 Description",
            Shelf = new Shelf()
            {
                Location = "Linz"
            }
        };
        
        target.Context.Things.Add(thing1);
        target.Context.Things.Add(thing2);
        await target.Context.SaveChangesAsync();
        
        // Arrange & Act
        var result = target.GetThingsByShortName("T1");
        var ok = result.Result as Ok<IEnumerable<Thing>>;
        
        // Assert
        Assert.IsType<Ok<IEnumerable<Thing>>>(result.Result);
        Assert.NotNull(ok);
        Assert.Equal(1, ok.Value.Count());
        Assert.Contains(thing1, ok.Value);
        await ClearDb();
    }
    
    
    [Fact]
    public async Task GivenThing_WhenCreateThing_ThenReturnCreated()
    {
        // Prepare
        await ClearDb();
        
        var shelf = new Shelf()
        {
            Location = "Vienna"
        };
        
        target.Context.Shelves.Add(shelf);
        await target.Context.SaveChangesAsync();
        
        // Arrange
        var thing = new ThingDto()
        {
            SerialNr = "789",
            ShortName = "T3",
            Description = "Thing3 Description",
            AgeRestriction = 18,
            ShelfId = shelf.Id
        };
        
        // Act
        var result = await target.CreateThing(thing);
        var created = result.Result as Created<int>;
        
        // Assert
        Assert.IsType<Created<int>>(result.Result);
        Assert.NotNull(created);
        Assert.NotEqual(0, created.Value);
        await ClearDb();
    }
    
    [Fact]
    public async Task GivenInvalidThing_WhenUpdateThing_ThenReturnBadRequest()
    {
        await ClearDb();
        
        // Arrange
        var thing = new ThingDataDto();
        
        // Act
        var result = await target.UpdateThing(thing);
        
        // Assert
        Assert.IsType<BadRequest>(result.Result);
        await ClearDb();
    }

    
    [Fact]
    public async Task GivenThing_WhenUpdateThing_ThenReturnOk()
    {
        // Prepare
        await ClearDb();
        
        Thing thing = new Thing()
        {
            SerialNr = "789",
            ShortName = "T3",
            Description = "Thing3 Description",
            Shelf = new Shelf()
            {
                Location = "Vienna"
            }
        };
        
        target.Context.Things.Add(thing);
        await target.Context.SaveChangesAsync();
        target.Context.ChangeTracker.Clear();
        
        // Arrange
        var updatedThing = new ThingDataDto()
        {
            ThingId = thing.Id,
            ShortName = "ShortNameUpdate",
        };
        
        // Act
        var result = await target.UpdateThing(updatedThing);
        
        // Assert
        Assert.IsType<Ok>(result.Result);
        await ClearDb();
    }
    
    
    [Fact]
    public async Task GivenThingWithImage_WhenGetImageById_ThenReturnOkWithImage()
    {
        await ClearDb();
        
        // Arrange
        var thing = new Thing()
        {
            SerialNr = "789",
            ShortName = "T3",
            Description = "Thing3 Description",
            Shelf = new Shelf()
            {
                Location  = "Vienna"
            },
            ThingDetails = new ThingDetails()
            {
                Image = new Image()
                {
                    Data = new byte[] { 0x00, 0x01, 0x02 }
                }
            }
        };
        
        target.Context.Things.Add(thing);
        await target.Context.SaveChangesAsync();
        
        // Act
        var result = target.GetImageById(thing.Id);
        var ok = result.Result as Ok<byte[]>;
        
        // Assert
        Assert.IsType<Ok<byte[]>>(result.Result);
        Assert.NotNull(ok);
        Assert.Equal(thing.ThingDetails.Image.Data, ok.Value);
        await ClearDb();
    }
    
    [Fact]
    public async Task GivenThingWithImage_WhenDeleteImage_ThenReturnOk()
    {
        // Prepare
        await ClearDb();
        
        var thing = new Thing()
        {
            SerialNr = "789",
            ShortName = "T3",
            Description = "Thing3 Description",
            Shelf = new Shelf()
            {
                Location  = "Vienna"
            },
            ThingDetails = new ThingDetails()
            {
                Image = new Image()
                {
                    Data = new byte[] { 0x00, 0x01, 0x02 }
                }
            }
        };
        
        target.Context.Things.Add(thing);
        await target.Context.SaveChangesAsync();
        
        // Arrange & Act
        var result = await target.DeleteImage(thing.Id);
        
        // Assert
        Assert.IsType<Ok>(result.Result);
        await ClearDb();
    }
    
    [Fact]
    public async Task GivenNull_WhenCreateImage_ThenReturnBadRequest()
    {
        await ClearDb();
        
        // Arrange & Act
        var result = await target.CreateImage(null);
        
        // Assert
        Assert.IsType<BadRequest>(result.Result);
        await ClearDb();
    }
    
    [Fact]
    public async Task GivenThingWithoutThingDetails_WhenCreateImage_ThenReturnNotFound()
    {
        // Prepare
        await ClearDb();
        
        var thing = new Thing()
        {
            SerialNr = "789",
            ShortName = "T3",
            Description = "Thing3 Description",
            Shelf = new Shelf()
            {
                Location  = "Vienna"
            }
        };
        
        target.Context.Things.Add(thing);
        await target.Context.SaveChangesAsync();
        
        // Arrange & Act
        var result = await target.CreateImage(new ImageDto()
        {
            ThingId = thing.Id,
            Blob = new byte[] { 0x00, 0x01, 0x02 }
        });
        
        // Assert
        Assert.IsType<NotFound>(result.Result);
        await ClearDb();
    }
    
}