namespace Core.Entities.DTO;

public class RentalDataDto
{
    public int LendingId { get; set; }
    public int? UserId { get; set; }
    public int? ThingId { get; set; }
    public int? LendingDurationDays { get; set; }
    public DateTime? HandInDate { get; set; }
}