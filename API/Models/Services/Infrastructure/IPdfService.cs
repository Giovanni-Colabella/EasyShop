using System;
using API.Models.Entities;

namespace API.Models.Services.Infrastructure;

public interface IPdfService
{
    byte[] GenerateOrderReceipt(Ordine ordine);
    byte[] GenerateClientList(List<Cliente> clienti);
    byte[] GenerateProductList(List<Prodotto> prodotti);
    byte[] GenerateSalesReport(List<Ordine> ordini, DateTime start, DateTime end);
}
