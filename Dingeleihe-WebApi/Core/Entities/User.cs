using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index(nameof(Email), IsUnique = true)]
public class User : BaseEntity
{
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [JsonIgnore]
    public ICollection<Rental> Rentals { get; set; }
}