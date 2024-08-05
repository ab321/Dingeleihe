using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index(nameof(SerialNr), IsUnique = true)]
public class Thing : BaseEntity
{
    [Required] 
    public string ShortName { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public string SerialNr { get; set; }
    
    public ThingDetails? ThingDetails { get; set; }

    public ICollection<Rental> Rentals { get; set; }
    
    public Shelf Shelf { get; set; }
    public int ShelfId { get; set; }
}