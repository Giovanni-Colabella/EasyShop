using System.Net;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Newtonsoft.Json;

namespace Frontend.Pages.Clienti
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public ClienteViewModel Cliente { get; set; } = new ClienteViewModel();

        public List<string> ListValidationErrors { get; set; } = new();

        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()  // Corretto nome metodo
        {
            var token = Request.Cookies["jwtToken"];
            var client = _httpClientFactory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/clienti");

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/AccessoNegato");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Cliente.Nome ??= "";
            Cliente.Cognome ??= "";
            Cliente.Email ??= "";
            Cliente.NumeroTelefono ??= "";
            Cliente.Indirizzo.Via ??= "";
            Cliente.Indirizzo.Citta ??= "";
            Cliente.Indirizzo.CAP ??= "";

            var token = Request.Cookies["jwtToken"];
            var client = _httpClientFactory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync("api/clienti", Cliente);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/AccessoNegato");
            }

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            if (string.IsNullOrWhiteSpace(content))
            {
                ListValidationErrors.Add("Errore del server: risposta vuota.");
                return Page();
            }

            var validationErrors = JsonConvert.DeserializeObject<ValidationErrors>(content);

            if (validationErrors?.Errors != null && validationErrors.Errors.Count > 0)
            {
                foreach (var error in validationErrors.Errors)
                {
                    ListValidationErrors.Add(error);
                }
            }
            else
            {
                ListValidationErrors.Add("Errore del server: nessun dettaglio sugli errori.");
            }

            return Page();
        }
    }
}
