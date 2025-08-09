using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.MakeAdmin
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        [Required(ErrorMessage = "Il campo email è obbligatorio")]
        [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido")]
        public string Email { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "Seleziona un ruolo")]
        public string Role { get; set; } = "";

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsync(
                    "http://localhost:5150/api/Roles",
                    new StringContent(
                        JsonSerializer.Serialize(new { email = Email, role = Role }),
                        Encoding.UTF8,
                        "application/json"
                    )
                );

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = $"Ruolo {Role} assegnato correttamente a {Email}";
                    ModelState.Clear(); // Resetta il form
                }
                else
                {
                    ErrorMessage = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore di connessione: {ex.Message}";
            }

            return Page();
        }
    }
}