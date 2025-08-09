namespace API.Models.DTO
{
    public record ProdottoRequestDto
    {
        public string NomeProdotto { get; init; }
        public string Categoria { get; init; }
        public string Descrizione { get; init; }
        public decimal Prezzo { get; init; }
        public int Giacenza { get; init; }
        public IFormFile ImgFile { get; init; }

        // Il costruttore senza parametri deve necessariamente essere pubblico per il model binding
        public ProdottoRequestDto() { }

        private ProdottoRequestDto(string nomeProdotto, string categoria, string descrizione, decimal prezzo, int giacenza, IFormFile imgFile)
        {
            NomeProdotto = nomeProdotto;
            Categoria = categoria;
            Descrizione = descrizione;
            Prezzo = prezzo;
            Giacenza = giacenza;
            ImgFile = imgFile;
        }

        public static Builder CreateBuilder() => new Builder();

        public class Builder
        {
            private string _nomeProdotto;
            private string _categoria;
            private string _descrizione;
            private decimal _prezzo;
            private int _giacenza;
            private IFormFile _imgFile;

            public Builder SetNomeProdotto(string nomeProdotto) { _nomeProdotto = nomeProdotto; return this; }
            public Builder SetCategoria(string categoria) { _categoria = categoria; return this; }
            public Builder SetDescrizione(string descrizione) { _descrizione = descrizione; return this; }
            public Builder SetPrezzo(decimal prezzo) { _prezzo = prezzo; return this; }
            public Builder SetGiacenza(int giacenza) { _giacenza = giacenza; return this; }
            public Builder SetImgFile(IFormFile imgFile) { _imgFile = imgFile; return this; }

            // Costruisce l'oggetto senza validazione
            public ProdottoRequestDto Build()
            {
                var prodotto = new ProdottoRequestDto(_nomeProdotto, _categoria, _descrizione, _prezzo, _giacenza, _imgFile);
                return prodotto;
            }
        }
    }
}
