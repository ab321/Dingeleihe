namespace Core.Entities.DTO;

public class ThingDto
{
    public string ShortName { get; set; }
    public string Description { get; set; }
    public string SerialNr { get; set; }
    public int AgeRestriction { get; set; }
    
    // Added ShelfId, because otherwise there would be a foreign key constraint violation!
    public int ShelfId { get; set; }
}