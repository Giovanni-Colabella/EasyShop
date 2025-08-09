using System.ComponentModel.DataAnnotations;

namespace Frontend.ViewModels
{
    public class Verify2FARequestModel
    {
        [Required(ErrorMessage = "Il codice è obbligatorio")]
        public string code { get; set; } = "";
        [Required(ErrorMessage = "Il campo UserId è obbligatorio")]
        public string UserId { get; set; } = "";
    }
}
