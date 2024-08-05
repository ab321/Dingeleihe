namespace Core.Entities.DTO;

public class ThingDataDto
{
    public int ThingId { get; set; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public string? SerialNr { get; set; }
    public int? AgeRestriction { get; set; }
    public byte[]? ThingImage { get; set; }
}