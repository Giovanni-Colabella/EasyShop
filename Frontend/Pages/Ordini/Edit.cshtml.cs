using System.Net.Http.Headers;
using Frontend.Customizations.GlobalObjects;
using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Frontend.Pages.Ordini
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public OrdineViewModel Ordine { get; set; } = new OrdineViewModel();

        public List<string> ListValidationErrors { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("Api");
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/ordini/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/AccessoNegato");
            }

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            Ordine = JsonConvert.DeserializeObject<OrdineViewModel>(json);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");
            var token = Request.Cookies["jwtToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsJsonAsync($"api/ordini/{Ordine.IdOrdine}", Ordine);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/AccessoNegato");
            }

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("Index");
            }

            var content = await response.Content.ReadAsStringAsync();

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
                ListValidationErrors.Add($"Errore nella risposta del server. {ex.Message}");
            }

            return Page();
        }
    }
}
