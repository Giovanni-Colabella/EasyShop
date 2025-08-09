namespace Frontend.ViewModels;

public class OrdineViewModel
{
    public int IdOrdine { get; set; }
    public decimal TotaleOrdine { get; set; } = 0;
    public DateTime DataOrdine { get; set; }
    public string Stato { get; set; } = "";
    public string MetodoPagamento { get; set; } = "";
    public int ClienteId { get; set; }
    public string NomeCliente { get; set; } = "";
    public List<DettaglioOrdineViewModel> Details { get; set; } = new();
}

public class DettaglioOrdineViewModel
{
    // Identificativo del prodotto
    public int ProductId { get; set; }

    // Nome del prodotto
    public string ProductName { get; set; } = string.Empty;

    // Quantità del prodotto acquistata
    public int Quantity { get; set; }

    // Prezzo unitario del prodotto
    public decimal Price { get; set; }

    // Prezzo totale (Quantity * Price)
    public decimal TotalPrice => Quantity * Price;
}