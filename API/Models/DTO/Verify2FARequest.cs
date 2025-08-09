using System.ComponentModel.DataAnnotations;

namespace API.Models.DTO;

public record class Verify2FARequest
{
    [Required(ErrorMessage = "IL codice è obbligatorio")]
    public string code { get; set; } = "";

    [Required(ErrorMessage = "Il campo UserId è obbligatorio")]
    public string UserId { get; set; } = "";
}
