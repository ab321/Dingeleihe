using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Image : BaseEntity
{
    [Required]
    public ThingDetails ThingDetails { get; set; }
    public int ThingDetailsId { get; set; }
    
    public byte[] Data { get; set; }
}