using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Shelf : BaseEntity
{
    [Required]
    public String Location { get; set; }
    
    public ICollection<Thing> Things { get; set; }
}