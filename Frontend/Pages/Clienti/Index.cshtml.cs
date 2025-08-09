using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyProject.Pages.Clienti
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const int PageSize = 10;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ClienteViewModel>? Clienti { get; set; } = new List<ClienteViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? Search { get; set; }

        public async Task OnGetAsync(string? search, int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            Search = search;

            var token = Request.Cookies["jwtToken"];
            var client = _httpClientFactory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string url = "api/clienti";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url = $"api/clienti/search?keyword={Uri.EscapeDataString(search)}";
            }

            var response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                Response.Redirect("/AccessoNegato");
                return;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var clienti = JsonSerializer.Deserialize<List<ClienteViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (clienti != null)
                {
                    var totalItems = clienti.Count;
                    TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
                    Clienti = clienti.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
                }
            }
            else
            {
                Clienti = new List<ClienteViewModel>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var token = Request.Cookies["jwtToken"];
            var client = _httpClientFactory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/clienti/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/AccessoNegato");
            }

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage(new { search = Search, pageNumber = CurrentPage });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Errore durante l'eliminazione del cliente.");
                return Page();
            }
        }
    }
}
