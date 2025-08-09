using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Frontend.Pages.Prodotti
{
    public class DetailModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public DetailModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
            _configuration = configuration;
        }

        [BindProperty]
        public ProdottoViewModel Prodotto { get; set; } = new();

        public string GetImmagineUrl(string nomeFile)
            => $"http://localhost:5150/{nomeFile}";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"api/prodotti/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/AccessoNegato");
            }

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            Prodotto = JsonSerializer.Deserialize<ProdottoViewModel>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
            return Page();
        }

        public async Task<IActionResult> OnPostPayPalAsync()
        {
            // 1) Prepara il DTO
            var dto = new PagamentoViewModel
            {
                Amount = Prodotto.Prezzo,
                Currency = "EUR",
                ReturnUrl = $"{Request.Scheme}://{Request.Host}/payment/paypal/PayPalConfirmed",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/",
                Prodotti = new List<ProdottoViewModel>
                {
                    new ProdottoViewModel
                    {
                        Id = Prodotto.Id,
                        Quantita = 1,
                        Prezzo = Prodotto.Prezzo,
                    }
                }
            };

            // 2) Token e header
            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3) Serializza e POST
            var jsonBody = JsonSerializer.Serialize(dto);
            using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var resp = await _httpClient.PostAsync("api/payment/create", content);

            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Errore pagamento: {resp.StatusCode} - {err}");
                return Page();
            }

            // 4) Deserializza risposta
            var responseJson = await resp.Content.ReadAsStringAsync();
            var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(
                responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (paymentResponse == null || string.IsNullOrEmpty(paymentResponse.OrderId))
            {
                ModelState.AddModelError("", "Risposta PayPal non valida.");
                return Page();
            }

            // 5) Redirect a PayPal
            var paypalUrl = $"{_configuration["PayPalUrl"]}/checkoutnow?token={paymentResponse.OrderId}";
            return Redirect(paypalUrl);
        }

        public class PaymentResponse
        {
            public string OrderId { get; set; } = string.Empty;
        }
    }
}
