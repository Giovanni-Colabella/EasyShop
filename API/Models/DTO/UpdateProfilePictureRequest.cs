using System.ComponentModel.DataAnnotations;

namespace API.Models.DTO;

public record class UpdateProfilePictureRequest
{
    [Required(ErrorMessage = "Il campo UserId non può essere vuoto")]
    public string UserId {get; set;} = "";
    public IFormFile? ImgFile { get; set; }
}
