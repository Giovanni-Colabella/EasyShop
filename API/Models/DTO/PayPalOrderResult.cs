using System;

namespace API.Models.DTO;

public class PayPalOrderResult
{
    public string OrderId { get; set; }
    public string Status { get; set; }
    public string PayerEmail { get; set; }
    public string Amount { get; set; }
    public string Currency { get; set; }
    public string IdProdotto { get; set; }
}
