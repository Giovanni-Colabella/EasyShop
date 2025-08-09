using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Auth
{
    public class RecuperoPasswordModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public RecuperoPasswordModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        [BindProperty]
        public string Email { get; set; } = "";

        public string Message { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Inserisci un indirizzo email valido.";
                return Page();
            }

            var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new { Email });

            if (response.IsSuccessStatusCode)
            {
                Message = "Email di recupero inviata, controlla la casella mail.";
            }
            else
            {
                Message = "Errore durante l'invio. Controlla che la mail sia valida.";
            }

            return Page();
        }
    }
}
