using API.Models.DTO;
using API.Models.Options;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using System.Globalization;

namespace API.Models.Services.Application
{
    public class PayPalService
    {
        private readonly IOptionsMonitor<PayPalSettings> config;
        private readonly ApplicationDbContext _dbContext;

        public PayPalService(IOptionsMonitor<PayPalSettings> config,
            ApplicationDbContext dbContext)
        {
            this.config = config;
            _dbContext = dbContext;
        }

        public async Task<string> CreateOrderAsync(PagamentoRequest pagamento)
        {
            try
            {
                if (pagamento.Prodotti == null || !pagamento.Prodotti.Any())
                    throw new ArgumentException("La lista dei prodotti non può essere vuota.");

                if (string.IsNullOrWhiteSpace(pagamento.Currency))
                    throw new ArgumentException("La valuta deve essere specificata.");

                var productIds = pagamento.Prodotti.Select(p => p.Id).ToList();

                var prodottiDb = await _dbContext.Prodotti
                    .Where(p => productIds.Contains(p.IdProdotto))
                    .ToListAsync();

                if (!prodottiDb.Any())
                    throw new ArgumentException("Nessun prodotto trovato per gli ID forniti.");

                // Costruzione elenco item PayPal con quantità
                var items = new List<Item>();
                decimal totale = 0;

                foreach (var prodottoRichiesto in pagamento.Prodotti)
                {
                    var prodottoDb = prodottiDb.FirstOrDefault(p => p.IdProdotto == prodottoRichiesto.Id);
                    if (prodottoDb == null)
                        continue;

                    var prezzoUnitario = prodottoDb.Prezzo;
                    var quantita = prodottoRichiesto.Quantita;

                    if (quantita <= 0)
                        throw new ArgumentException($"Quantità non valida per il prodotto {prodottoDb.NomeProdotto}");

                    decimal subTotal = prezzoUnitario * quantita;
                    totale += subTotal;

                    items.Add(new Item
                    {
                        Name = prodottoDb.NomeProdotto,
                        UnitAmount = new Money
                        {
                            CurrencyCode = pagamento.Currency,
                            Value = prezzoUnitario.ToString("0.00", CultureInfo.InvariantCulture)
                        },
                        Quantity = quantita.ToString()
                    });
                }

                var purchaseUnit = new PurchaseUnitRequest
                {
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = pagamento.Currency,
                        Value = totale.ToString("0.00", CultureInfo.InvariantCulture),
                        AmountBreakdown = new AmountBreakdown
                        {
                            ItemTotal = new Money
                            {
                                CurrencyCode = pagamento.Currency,
                                Value = totale.ToString("0.00", CultureInfo.InvariantCulture)
                            }
                        }
                    },
                    Items = items
                };

                var order = new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits = new List<PurchaseUnitRequest> { purchaseUnit },
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = pagamento.ReturnUrl,
                        CancelUrl = pagamento.CancelUrl
                    }
                };

                var environment = new SandboxEnvironment(config.CurrentValue.ClientId, config.CurrentValue.ClientSecret);
                var client = new PayPalHttpClient(environment);

                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                request.RequestBody(order);

                var response = await client.Execute(request);
                var result = response.Result<Order>();

                return result.Id;
            }
            catch (HttpException ex)
            {
                var body = ex.Message;
                var debugId = ex.Headers.TryGetValues("PayPal-Debug-Id", out var debugHeader)
                    ? debugHeader.FirstOrDefault()
                    : "N/A";

                Console.WriteLine("❌ PayPal API Error:");
                Console.WriteLine($"Status Code: {ex.StatusCode}");
                Console.WriteLine($"Body: {body}");
                Console.WriteLine($"Debug Id: {debugId}");

                throw new Exception($"PayPal CreateOrderAsync failed. StatusCode: {ex.StatusCode}, Body: {body}, DebugId: {debugId}");
            }
        }

        public async Task<PayPalOrderResult?> CaptureOrderAsync(string orderId)
        {
            var environment = new SandboxEnvironment(config.CurrentValue.ClientId, config.CurrentValue.ClientSecret);
            var client = new PayPalHttpClient(environment);
            var request = new OrdersCaptureRequest(orderId);
            request.RequestBody(new OrderActionRequest());

            var response = await client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created &&
                response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var result = response.Result<Order>();

            if (result.Status != "COMPLETED")
                return null;

            return new PayPalOrderResult
            {
                OrderId = result.Id,
                Status = result.Status,
                Amount = result.PurchaseUnits.First().Payments.Captures.First().Amount.Value,
                Currency = result.PurchaseUnits.First().Payments.Captures.First().Amount.CurrencyCode,
            };
        }
    }
}
