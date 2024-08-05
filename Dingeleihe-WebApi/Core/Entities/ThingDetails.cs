using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class ThingDetails : BaseEntity
{
    [Required]
    public Thing Thing { get; set; }
    public int ThingId { get; set; }
    
    [Required]
    public int AgeRestriction { get; set; }
    
    public Image Image { get; set; }
}