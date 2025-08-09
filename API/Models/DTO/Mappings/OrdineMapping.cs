using API.Models.Entities;

namespace API.Models.DTO.Mappings;

public static  class OrdineMapping
{
    public static Ordine ToEntity(this OrdineDto ordineDto)
    {
        return new Ordine
        {
            IdOrdine = ordineDto.IdOrdine,
            TotaleOrdine = ordineDto.TotaleOrdine,
            DataOrdine = ordineDto.DataOrdine,
            Stato = ordineDto.Stato,
            MetodoPagamento = ordineDto.MetodoPagamento,
            ClienteId = ordineDto.ClienteId,
        };
    }

    public static OrdineDto ToDto(this Ordine ordine)
    {
        return new OrdineDto(
            ordine.IdOrdine,
            ordine.DataOrdine,
            ordine.ClienteId,
            ordine.TotaleOrdine,
            ordine.Stato,
            ordine.MetodoPagamento,
            ordine.Cliente.Nome + " " + ordine.Cliente.Cognome
        );
    }
}
