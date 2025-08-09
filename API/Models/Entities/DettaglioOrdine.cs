using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Entities
{
    // Questa tabella è usata per mappare la relazione Many To Many di Ordine e Prodotto
    public class DettaglioOrdine
    {
        [Key]
        public int IdDettaglio { get; set; }

        [ForeignKey("Ordine")]
        public int OrdineId { get; set; }
        public Ordine Ordine { get; set; } = null!;

        [ForeignKey("Prodotto")]
        public int ProdottoId { get; set; }
        public Prodotto Prodotto { get; set; } = null!;

        public int Quantita { get; set; }

    }
}