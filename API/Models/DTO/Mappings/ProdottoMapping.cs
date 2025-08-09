using API.Models.Entities;

namespace API.Models.DTO.Mappings;

public static class ProdottoMapping
{   
    public static Prodotto ToEntity(this ProdottoRequestDto prodottoRequestDto)
    {
        return new Prodotto 
        {
            NomeProdotto = prodottoRequestDto.NomeProdotto,
            Categoria = prodottoRequestDto.Categoria,
            Descrizione = prodottoRequestDto.Descrizione,
            Prezzo = prodottoRequestDto.Prezzo,
            Giacenza = prodottoRequestDto.Giacenza
        };
    }

    public static ProdottoResponseDto ToDto(this Prodotto prodotto)
    {
        return new ProdottoResponseDto
        {
            NomeProdotto = prodotto.NomeProdotto,
            Id = prodotto.IdProdotto,
            Prezzo = prodotto.Prezzo,
            Categoria = prodotto.Categoria,
            Descrizione = prodotto.Descrizione,
            Giacenza = prodotto.Giacenza,
            ImgPath = prodotto.ImgPath
        };
    }
}
