using Newtonsoft.Json;

namespace Frontend.ViewModels
{
    public class ProdottoViewModel
    {
        public int Id { get; set; } 
        public string NomeProdotto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public decimal Prezzo { get; set; }
        public int Giacenza { get; set; }
        public int Quantita { get; set; }
        public int QuantitaDisponibile { get; set; }
        public DateTime DataInserimento { get; set; }
        public string ImgPath { get; set; } = string.Empty;
    }
}
