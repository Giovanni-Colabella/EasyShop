using System.Net;
using System.Net.Http.Headers;
using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Frontend.Pages.Carrello
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public IndexModel(IHttpClientFactory httpClient, IConfiguration config)
        {
            _httpClient = httpClient.CreateClient("Api");
            _config = config;
        }

        public List<ProdottoViewModel> Articoli { get; set; } = new();

        public string GetImmagineUrl(string nomeFile)
        {

            return $"http://localhost:5150/{nomeFile}";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var jwtToken = Request.Cookies["jwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.GetAsync("api/carrello");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Articoli = JsonConvert.DeserializeObject<List<ProdottoViewModel>>(content) ?? new();
                return Page();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/Auth/Login");
            }

            return RedirectToPage("/Error");
        }

        public async Task<IActionResult> OnPostPayFromCartAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Request.Cookies["jwtToken"]);

            var response = await _httpClient.PostAsync("api/payment/payment-from-cart", null);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Auth/Login");

                return RedirectToPage("/Payment/PayPal/PaymentError");
            }

            var result = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<PagamentoResponseViewModel>(result);

            string? paypalUrl = _config["PayPalUrl"];
            return Redirect($"{paypalUrl}/checkoutnow?token={data?.OrderId}");
        }

        public class PagamentoResponseViewModel
        {
            public string OrderId { get; set; } = "";
        }
    }
}
