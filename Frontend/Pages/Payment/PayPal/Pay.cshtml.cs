using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace Frontend.Pages.Payment.PayPal
{
    public class PaymentModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PaymentModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public int ProdottoId { get; set; }

        [BindProperty]
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [BindProperty]
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "EUR";

        public List<ProdottoViewModel> Prodotti { get; set; } = new();

        [BindProperty]
        public PagamentoViewModel PagamentoRequest { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await RecuperaDatiPagamento();
            return result ?? Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await RecuperaDatiPagamento();
                return Page();
            }

            // Imposta sempre almeno 1 di quantità per ogni prodotto  
            PagamentoRequest.Prodotti ??= new List<ProdottoViewModel>();
            foreach (var p in PagamentoRequest.Prodotti)
            {
                if (p.Quantita <= 0) p.Quantita = 1;
            }

            try
            {
                var token = Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(token))
                    return RedirectToPage("/Account/Login");

                var client = _httpClientFactory.CreateClient("Api");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Preparo il request DTO direttamente da PagamentoRequest  
                PagamentoRequest = new PagamentoViewModel
                {
                    Amount = Amount,
                    Currency = Currency,
                    ReturnUrl = $"{Request.Scheme}://{Request.Host}/payment/paypal/PayPalConfirmed",
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/",
                    Prodotti = PagamentoRequest.Prodotti
                };

                var paymentResponse = await client.PostAsJsonAsync("api/payment/create", PagamentoRequest);

                if (!paymentResponse.IsSuccessStatusCode)
                {
                    var error = await paymentResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Errore PayPal API: {paymentResponse.StatusCode} - {error}");
                }

                var resultContent = await paymentResponse.Content.ReadFromJsonAsync<PaymentResponse>();
                var paypalUrl = $"{_configuration["PayPalUrl"]}/checkoutnow?token={resultContent?.OrderId}";

                return Redirect(paypalUrl);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Errore pagamento: {ex.Message}");
                await RecuperaDatiPagamento();
                return Page();
            }
        }

        private async Task<IActionResult?> RecuperaDatiPagamento()
        {
            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Sessione scaduta, effettuare il login.");
                return Page();
            }

            var client = _httpClientFactory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/payment/get-payment-request?prodottoId={ProdottoId}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", $"Errore API: {response.StatusCode}");
                return Page();
            }

            PagamentoRequest = await response.Content.ReadFromJsonAsync<PagamentoViewModel>() ?? new();
            Amount = PagamentoRequest.Amount;
            Currency = PagamentoRequest.Currency;
            Prodotti = PagamentoRequest.Prodotti;

            return null;
        }

        public class PaymentResponse
        {
            public string OrderId { get; set; } = string.Empty;
        }
    }
}
