using System.Net.Http.Headers;
using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Frontend.Pages.Ordini
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const int PageSize = 10;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public List<OrdineViewModel> Ordini { get; set; } = new List<OrdineViewModel>();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? Search { get; set; }

        public async Task OnGetAsync(string? search, int? pageNumber)
        {
            Search = search;
            CurrentPage = pageNumber ?? 1;

            var client = _httpClientFactory.CreateClient("Api");

            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string url = "api/ordini";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url = $"api/ordini/search?keyword={Uri.EscapeDataString(search)}";
            }

            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Response.Redirect("/AccessoNegato");
                return;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var ordini = JsonConvert.DeserializeObject<List<OrdineViewModel>>(json);

                if (ordini != null)
                {
                    var totalOrdini = ordini.Count;
                    TotalPages = (int)Math.Ceiling(totalOrdini / (double)PageSize);

                    Ordini = ordini.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/ordini/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/AccessoNegato");
            }

            if (response.IsSuccessStatusCode)
            {
                // Ricarica pagina mantenendo eventuali parametri di ricerca e paginazione se vuoi
                return RedirectToPage();
            }
            else
            {
                return Page();
            }
        }
    }
}
