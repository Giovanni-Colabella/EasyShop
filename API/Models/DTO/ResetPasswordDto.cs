namespace API.Models.DTO;

public record class ResetPasswordDto
{
    public string UserId { get; init; }
    public string Token { get; init; }
    public string NuovaPassword { get; init; }
}
