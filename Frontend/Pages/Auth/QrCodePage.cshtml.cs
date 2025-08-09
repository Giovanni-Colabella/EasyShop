using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Frontend.Pages.Auth
{
    public class QrCodePageModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public QrCodePageModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        [BindProperty]
        public Verify2FARequestModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = "";

        public ApplicationUserViewModel User { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                ErrorMessage = "Utente non autenticato";
                return Page();
            }

            var response = await _httpClient.GetAsync("api/auth/getUser");

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Errore nel recupero dati dal server";
                return Page();
            }

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                User = JsonConvert.DeserializeObject<ApplicationUserViewModel>(content);
                Input.UserId = User.UserId;
            }
            catch
            {
                ErrorMessage = "Errore generico del server";
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCodeAppAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Errore: Campi non compilati";
                return Page();
            }

            var token = Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                ErrorMessage = "Utente non autenticato";
                return Page();
            }

            var response = await _httpClient.PostAsJsonAsync("api/auth/verify-2fa", Input);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                ErrorMessage = "Errore: Richiesta non autenticata";
                return Page();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                ErrorMessage = "Errore: Codice non valido";
                return Page();
            }

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Auth/AccountManager");
            }

            ErrorMessage = "Errore: Qualcosa è andato storto, riprova";
            return Page();
        }
    }
}
