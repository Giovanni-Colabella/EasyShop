using System.ComponentModel.DataAnnotations;

namespace API.Models.Entities;

public class CarrelloProdotto
{
    public int CarrelloId { get; set; }
    public Carrello Carrello { get; set; }

    public int ProdottoId { get; set; }
    public Prodotto Prodotto { get; set; }

    public int Quantita { get; set; } = 1;
}
