using System.ComponentModel.DataAnnotations;

namespace API.Models.Entities;

public class BannedIp
{
    [Key]
    public int Id { get; set; }
    public string Ip { get; set; } = "";
    public string Motivazione { get; set; } = "";
}
