using System.ComponentModel.DataAnnotations;

namespace API.Models.Entities;

public class ProfilePicture
{
    [Key]
    public string UserId { get; set; } = "";
    public string ImgPath { get; set; } = "";
}
