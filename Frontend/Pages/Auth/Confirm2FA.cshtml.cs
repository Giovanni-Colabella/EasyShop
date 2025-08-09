using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Frontend.Pages.Auth
{
    public class Confirm2FAModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public Confirm2FAModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
            _config = config;
        }

        [BindProperty]
        public Verify2FARequestModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task OnGet()
        {
            if (Request.Cookies.TryGetValue("cookie_UserId", out var userId))
            {
                Input.UserId = userId;
            }
            else
            {
                ErrorMessage = "Errore: Impossibile recuperare ID utente";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Codice non valido. Riprova.";
                return Page();
            }

            // Usa solo il path, la base URL è già configurata in HttpClient "Api"
            var response = await _httpClient.PostAsJsonAsync("api/auth/confirm-2fa", Input);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Codice non valido";
                return Page();
            }

            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    Response.Headers.Append("Set-Cookie", cookie);
                }
            }

            return RedirectToPage("/Index");
        }
    }
}
