namespace API.Models.DTO;

public record class UserAddressResponseDto
{
    public string Indirizzo_Citta { get; init; } = string.Empty;
    public string Indirizzo_Via  { get; set; } = string.Empty;
    public string Indirizzo_CAP { get; set; } = string.Empty;
    public string Indirizzo_HouseNumber { get; set; } = string.Empty;
}
