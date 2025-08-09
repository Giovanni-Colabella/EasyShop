using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Frontend.Pages.Payment
{
    public class OrdiniModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public List<OrdineViewModel> Ordini { get; set; } = new();

        public OrdiniModel(IHttpClientFactory httpClient, IConfiguration config)
        {
            _httpClient = httpClient.CreateClient("Api");
            _config = config;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Request.Cookies["jwtToken"]);

            var response = await _httpClient.GetAsync("api/payment/get-my-orders");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Ordini = JsonConvert.DeserializeObject<List<OrdineViewModel>>(content)
                    ?? new List<OrdineViewModel>();
                return Page();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/Auth/Login");
            }

            return Page();
        }
    }
}
