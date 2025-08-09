using API.Models.Enums;
using API.Models.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace API.Models.Entities;

public class Ordine
{
    [Key]
    public int IdOrdine { get; set; }
    public decimal TotaleOrdine { get; set; } = 0;
    public DateTime DataOrdine { get; set; }
    public  OrdineStatus Stato { get; set; } = OrdineStatus.Sospeso;
    public string MetodoPagamento { get; set; } = "";
    public string? IndirizzoSpedizione { get; set; } = string.Empty;
    public string? PayPalOrdineId { get; set; } = null;
        
    // Foreign key
    public int ClienteId { get; set; }
    // Proprietà di navigazione
    public Cliente Cliente { get; set; } = null!; // Ho usato il null forgiving operator per specificare che sono sicuro che questa var non sarà mai nulla


    // Relazione many-to-many con Prodotto 
    public List<DettaglioOrdine> DettagliOrdini { get; set; } = null!;
}
