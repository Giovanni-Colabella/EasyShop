using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Entities;

public class Carrello
{
    [Key]
    public int Id { get; set; }

    public string ClienteId { get; set; }
    
    [ForeignKey("ClienteId")]
    public ApplicationUser Cliente { get; set; }

    public List<CarrelloProdotto> CarrelloProdotti { get; set; }
}
