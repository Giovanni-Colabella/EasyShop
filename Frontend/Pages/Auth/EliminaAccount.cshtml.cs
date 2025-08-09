using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Auth
{
    public class EliminaAccountModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public EliminaAccountModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _client = httpClientFactory.CreateClient("Api");
            _config = config;
        }

        [BindProperty]
        public string Password { get; set; } = "";

        public string ErrorMessage { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "La password non può essere vuota";
                return Page();
            }

            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Token di autenticazione mancante.";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Usa solo path relativo: la base URL è già nel client "Api"
            var validateUrl = $"api/auth/validate-user-password?password={Uri.EscapeDataString(Password)}";
            var response = await _client.GetAsync(validateUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "Non sei autorizzato ad eseguire questa richiesta";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "La password non è valida";
                return Page();
            }

            var deleteResponse = await _client.DeleteAsync("api/auth/delete-user");

            if (!deleteResponse.IsSuccessStatusCode)
            {
                ErrorMessage = "Errore durante l'eliminazione dell'account.";
                return Page();
            }

            var logoutResponse = await _client.PostAsync("api/auth/logout", null);

            if (!logoutResponse.IsSuccessStatusCode)
            {
                ErrorMessage = "Ops, qualcosa è andato storto";
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }
}
