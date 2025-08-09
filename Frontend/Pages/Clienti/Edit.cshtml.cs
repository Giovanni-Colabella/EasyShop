using System.Net;
using System.Net.Http.Headers;
using Frontend.Customizations.GlobalObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Frontend.Pages.Clienti
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public ClienteViewModel Cliente { get; set; } = new ClienteViewModel();

        public List<string> ListValidationErrors { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = Request.Cookies["jwtToken"];
            var client = _httpClientFactory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/clienti/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/AccessoNegato");
            }

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            Cliente = JsonConvert.DeserializeObject<ClienteViewModel>(json);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Assicuriamoci di non inviare valori nulli
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

            var response = await client.PutAsJsonAsync($"api/clienti/{Cliente.Id}", Cliente);

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

            try
            {
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
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine($"Errore durante la deserializzazione: {ex.Message}");
                ListValidationErrors.Add("Errore nella risposta del server.");
            }

            return Page();
        }
    }

    // Aggiungi la classe ValidationErrors se non l'hai già definita
    public class ValidationErrors
    {
        public List<string> Errors { get; set; } = new List<string>();
    }
}
