namespace API.Models.DTO;

public record class UpdateAccountRequestDto
{
    public string UserId { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Cognome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Indirizzo_Citta { get; init; } = string.Empty;
    public string? Indirizzo_Via { get; init; } = string.Empty;
    public string? Indirizzo_CAP { get; init; } = string.Empty;
    public string? Indirizzo_HouseNumber { get; init; } = string.Empty;
}