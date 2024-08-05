using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Rental : BaseEntity
{
    public Thing Thing { get; set; }
    public int ThingId { get; set; }
    
    public User User { get; set; }
    public int UserId { get; set; }
    
    [Required]
    public DateTime From { get; set; }
    
    [Required]
    public DateTime Until { get; set; }
    
    public DateTime? ReturnedOn { get; set; }
    
    //public bool IsReturned => ReturnedOn.HasValue;
}