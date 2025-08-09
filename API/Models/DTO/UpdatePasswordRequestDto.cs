namespace API.Models.DTO;

public record class UpdatePasswordRequestDto
{
    public string? UserId { get; init; }
    public string? PasswordCorrente { get; init; } 
    public string? NuovaPassword { get; init; } 
    public string? ConfermaPassword { get; set; }
}
