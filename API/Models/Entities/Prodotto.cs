using System.ComponentModel.DataAnnotations;

namespace API.Models.Entities
{
    public class Prodotto
    {
        private int _quantitaDisponibile;


        [Key]
        public int IdProdotto { get; set; }
        [Required]
        public string NomeProdotto { get; set; } = "";
        [Required]
        public string Categoria { get; set; } = "";
        public string Descrizione { get; set; } = "";   
        [Required]
        public decimal Prezzo { get; set; } = 0.00M;
        [Required]
        public int Giacenza { get; set; } = 1;
        
        public int QuantitaDisponibile
        {
            get => _quantitaDisponibile;
            set
            {
                _quantitaDisponibile = value > Giacenza ? Giacenza : value;
            }
        }

        // Caricamento Immagine
        public string ImgPath { get; set; } = "";
        // Relazione many-to-many con Ordine 
        public List<DettaglioOrdine> DettagliOrdini { get; set; } = new();

        public List<CarrelloProdotto> CarrelloProdotti { get; set; } = new();
        
    }
}