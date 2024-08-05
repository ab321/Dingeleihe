using Core.Entities;
using Core.Entities.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Dingeleihe_WebApi;

public class ThingController
{
    public ThingController(ApplicationDbContext context)
    {
        Context = context;
        Things = context.Things;
    }
    
    public IQueryable<Thing> Things { get; }
    public ApplicationDbContext Context { get; set; }
    
    public Results<NotFound, Ok<IEnumerable<Thing>>> GetThings()
    {
        var things = Things.ToList();
        return things.Count > 0 ? TypedResults.Ok(things.AsEnumerable()) : TypedResults.NotFound();
    }
    
    public Results<NotFound, Ok<Thing>> GetThingById(int id)
    {
        var thing = Things.FirstOrDefault(t => t.Id == id);
        return thing != null ? TypedResults.Ok(thing) : TypedResults.NotFound();
    }
    
    public async Task<Results<BadRequest<String>, NotFound, Ok>> DeleteThingById(int id)
    {
        var thing = await Things.FirstOrDefaultAsync(t => t.Id == id);
        if (thing == null)
            return TypedResults.BadRequest("Thing not found");
        
        if(thing.Rentals != null && thing.Rentals.Count > 0)
            Context.Rentals.RemoveRange(thing.Rentals);

        if (thing.ThingDetails != null)
        {
            if (thing.ThingDetails.Image != null)
                Context.ImageDetails.Remove(thing.ThingDetails.Image);
            
            Context.ThingDetails.Remove(thing.ThingDetails);
        }
        
        Context.Things.Remove(thing);
        await Context.SaveChangesAsync();
        return TypedResults.Ok();
    }
    
    public Results<BadRequest<String>, NotFound, Ok<IEnumerable<Thing>>> GetThingsByShortName(string shortName)
    {
        if (string.IsNullOrEmpty(shortName))
            return TypedResults.BadRequest("ShortName is empty");
        
        var things = Things.Where(t => t.ShortName == shortName).ToList();
        return things.Count > 0 ? TypedResults.Ok(things.AsEnumerable()) : TypedResults.NotFound();
    }
    
    public async Task<Results<BadRequest, Created<int>>> CreateThing(ThingDto thing)
    {
        if (thing == null)
            return TypedResults.BadRequest();
        
        var shelf = Context.Shelves.FirstOrDefault(s => s.Id == thing.ShelfId);
        if (shelf == null)
            return TypedResults.BadRequest();
        
        if (string.IsNullOrEmpty(thing.ShortName) || 
            string.IsNullOrEmpty(thing.Description) || 
            string.IsNullOrEmpty(thing.SerialNr) || 
            thing.AgeRestriction < 0)
            return TypedResults.BadRequest();
        
        var thingDetails = new ThingDetails { AgeRestriction = thing.AgeRestriction };
        var newThing = new Thing
        {
            ShortName = thing.ShortName,
            Description = thing.Description,
            SerialNr = thing.SerialNr,
            ThingDetails = thingDetails,
            Shelf = shelf,
            ShelfId = shelf.Id
        };
        
        await Context.Things.AddAsync(newThing);
        await Context.SaveChangesAsync();
        return TypedResults.Created($"/things/{newThing.Id}", newThing.Id);
    }
    
    public async Task<Results<BadRequest, Created<int>>> CreateShelf(Shelf shelf)
    {
        if (shelf == null)
            return TypedResults.BadRequest();
        
        if (string.IsNullOrEmpty(shelf.Location))
            return TypedResults.BadRequest();
        
        await Context.Shelves.AddAsync(shelf);
        await Context.SaveChangesAsync();
        return TypedResults.Created($"/shelves/{shelf.Id}", shelf.Id);
    }
    public async Task<Results<BadRequest, NotFound, Ok>> UpdateThing(ThingDataDto thing)
    {
        if(!(thing.ThingId != null && 
           (thing.ShortName != null || thing.Description != null || thing.SerialNr != null || 
            thing.AgeRestriction != null || thing.ThingImage != null)))
            return TypedResults.BadRequest();

        var currentThing = Things.FirstOrDefault(t => t.Id == thing.ThingId);
        
        if(currentThing == null)
            return TypedResults.NotFound();
        
        if (thing.ShortName != null)
            currentThing.ShortName = thing.ShortName;
        if (thing.Description != null)
            currentThing.Description = thing.Description;
        if (thing.SerialNr != null)
            currentThing.SerialNr = thing.SerialNr;
        if (currentThing.ThingDetails != null)
        {
            if (thing.AgeRestriction != null)
                currentThing.ThingDetails.AgeRestriction = thing.AgeRestriction.Value;
            if (thing.ThingImage != null)
            {
                if (currentThing.ThingDetails.Image != null)
                {
                    currentThing.ThingDetails.Image.Data = thing.ThingImage;
                }
                else
                {
                    currentThing.ThingDetails.Image = new Image { Data = thing.ThingImage };
                }
            }
        }

        if (currentThing.ThingDetails == null)
        {
            if (thing.AgeRestriction != null)
            {
                currentThing.ThingDetails = new ThingDetails { AgeRestriction = thing.AgeRestriction.Value };
                
                if (thing.ThingImage != null)
                    currentThing.ThingDetails.Image = new Image { Data = thing.ThingImage };
            }
        }
        
        Context.Things.Update(currentThing);
        await Context.SaveChangesAsync();
        return TypedResults.Ok();
    }
    
    public Results<NotFound, Ok<byte[]>> GetImageById(int id)
    {
        var thing = Context.Things
            .Include(t => t.ThingDetails)
            .ThenInclude(td => td.Image)
            .FirstOrDefault(t => t.Id == id);
        
        if (thing == null || thing.ThingDetails == null || thing.ThingDetails.Image == null)
            return TypedResults.NotFound();
        
        return TypedResults.Ok(thing.ThingDetails.Image.Data);
    }
    
    public async Task<Results<NotFound, Ok>> DeleteImage(int id)
    {
        var thing = Context.Things
            .Include(t => t.ThingDetails)
            .ThenInclude(td => td.Image)
            .FirstOrDefault(t => t.Id == id);
       
        if (thing == null || thing.ThingDetails == null || thing.ThingDetails.Image == null)
            return TypedResults.NotFound();
        
        Context.ImageDetails.Remove(thing.ThingDetails.Image);
        await Context.SaveChangesAsync();
        return TypedResults.Ok();
    }
    
    public async Task<Results<NotFound, BadRequest, Created>> CreateImage(ImageDto imageDto)
    {
        if(imageDto == null || imageDto.Blob == null)
            return TypedResults.BadRequest();

        var thing = Context.Things
            .Include(t => t.ThingDetails)
            .ThenInclude(td => td.Image)
            .FirstOrDefault(t => t.Id == imageDto.ThingId);
        
        if (thing?.ThingDetails == null)
            return TypedResults.NotFound();
        
        
        if (thing.ThingDetails.Image != null)
            Context.ImageDetails.Remove(thing.ThingDetails.Image);
        
        Image image = new Image
        {
            ThingDetails = thing.ThingDetails,
            ThingDetailsId = thing.ThingDetails.Id,
            Data = imageDto.Blob
        };

        thing.ThingDetails.Image = image;
        Context.Things.Update(thing);
        await Context.SaveChangesAsync();
        return TypedResults.Created();
    }
}