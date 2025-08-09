using System.ComponentModel.DataAnnotations;

namespace API.Models.DTO;

public record class RoleRequestDto
{
    [Required(ErrorMessage = "Il campo email è obbligatorio")]
    [EmailAddress(ErrorMessage = "Il campo email deve essere un indirizzo email valido")]
    public string Email { get; init; } = "";
    [Required(ErrorMessage = "Il campo ruolo è obbligatorio")]
    [RegularExpression("Admin|User", ErrorMessage = "Il campo ruolo deve essere 'Admin' o 'User'")]
    public string Role { get; init; } = "";
}
