namespace API.Models.DTO;

public record class ApplicationUserDto
{
    public string UserId { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Cognome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool Is2FAEnabled { get; init; } = false;
}
