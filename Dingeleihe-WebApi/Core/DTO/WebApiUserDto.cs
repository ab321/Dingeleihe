namespace Core.Entities.DTO;

public class WebApiUserDto
{
    public string NormalizedEmail { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}