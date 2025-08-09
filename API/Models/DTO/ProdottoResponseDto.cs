namespace API.Models.DTO
{
    public record ProdottoResponseDto
    {

        public int     Id            { get; init; }
        public string  NomeProdotto  { get; init; } = "";
        public string  Categoria     { get; init; } = "";
        public string  Descrizione   { get; init; } = "";
        public decimal Prezzo        { get; init; }
        public int     Giacenza      { get; init; }
        public int QuantitaDisponibile { get; init; }
        public int Quantita { get; init; }
        public string ImgPath { get; init; } = "";
    }
}
