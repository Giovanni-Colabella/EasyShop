namespace API.Models.DTO;

public record class UpdateProfilePictureResponse
{
    public string UserId { get; set; } = "";
    public string? ImgPath { get; set; }
}
