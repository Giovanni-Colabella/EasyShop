using API.Models.Enums;

namespace API.Models.DTO;

public record OrdineDto (
    int IdOrdine, 
    DateTime DataOrdine, 
    int ClienteId, 
    decimal TotaleOrdine,
    OrdineStatus Stato,
    string MetodoPagamento, 
    string NomeCliente
);