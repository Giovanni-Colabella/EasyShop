using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Frontend.Pages.Prodotti
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {

            _httpClient = httpClientFactory.CreateClient("Api");
        }

        [BindProperty]
        public List<ProdottoViewModel> Prodotti { get; set; } = new();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? Search { get; set; }
        public string? CategoriaSelezionata { get; set; }

        public class ProdottoResponse
        {
            public int TotalCount { get; set; }
            public int PaginaCorrente { get; set; }
            public int TotalePagine { get; set; }
            public List<ProdottoViewModel>? Items { get; set; }
        }

        public string GetImmagineUrl(string nomeFile)
        {
            //var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost:5150/";+
            var baseUrl = "http://localhost:5150/";
            return string.IsNullOrWhiteSpace(nomeFile) ? baseUrl : $"{baseUrl}{nomeFile}";
        }

        public string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return text.Length > maxLength ? text.Substring(0, maxLength - 3) + "..." : text;
        }

        public async Task<IActionResult> OnGetAsync(int? pageNumber, string? search, string? categoria)
        {
            CurrentPage = pageNumber ?? 1;
            Search = search;
            CategoriaSelezionata = categoria;

            var url = $"api/prodotti?page={CurrentPage}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                url += $"&categoria={Uri.EscapeDataString(categoria)}";
            }

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProdottoResponse>(json);

                Prodotti = result?.Items ?? new List<ProdottoViewModel>();
                TotalPages = result?.TotalePagine ?? 1;
            }
            else
            {
                Prodotti = new List<ProdottoViewModel>();
                TotalPages = 1;
            }

            return Page();
        }
    }
}
