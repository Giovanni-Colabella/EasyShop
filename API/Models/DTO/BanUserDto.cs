namespace API.Models.DTO;

public record BanUserDto
{
    public string Email { get; init; } = "";
    public string Motivazione { get; init; } = "";
}
