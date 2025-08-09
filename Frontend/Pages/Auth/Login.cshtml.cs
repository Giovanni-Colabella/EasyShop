using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Newtonsoft.Json;

namespace Frontend.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
            // Assicurati che "Api" abbia la BaseAddress configurata correttamente altrove
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public class LoginInputModel
        {
            [Required(ErrorMessage = "Il campo 'Email' è obbligatorio")]
            [EmailAddress(ErrorMessage = "Il campo 'Email' deve essere un indirizzo email valido")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Il campo 'Password' è obbligatorio")]
            public string Password { get; set; } = "";
        }

        public List<string> ListValidationErrors { get; set; } = new();

        public class ValidationErrors
        {
            [JsonProperty("errors")]
            public List<string> Errors { get; set; } = new();
        }

        public void OnGet()
        {
            // Nessuna logica necessaria per GET
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var requestData = new StringContent(JsonConvert.SerializeObject(Input), Encoding.UTF8, "application/json");

            // Usa solo il path relativo, senza host/porta
            var response = await _httpClient.PostAsync("api/Auth/login", requestData);

            if (response.IsSuccessStatusCode)
            {
                // Propaga eventuali cookie di autenticazione dalla risposta dell'API
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    foreach (var cookie in cookies)
                    {
                        Response.Headers.Append("Set-Cookie", cookie);
                    }
                }

                return RedirectToPage("/Index");
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                // Prova a deserializzare risposta 2FA
                try
                {
                    var twoFactorResponse = JsonConvert.DeserializeObject<TwoFactorRequiredResponse>(errorContent);
                    if (twoFactorResponse?.Requires2FA == true)
                    {
                        // Imposta cookie per il flusso 2FA
                        Response.Cookies.Append("cookie_UserId", twoFactorResponse.UserId, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = false, // In produzione impostare a true, per localhost può essere false
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddHours(1)
                        });

                        return RedirectToPage("/Auth/Confirm2FA");
                    }
                }
                catch
                {
                    // Ignora e prova altra deserializzazione
                }

                // Prova a deserializzare errori di validazione
                try
                {
                    var validationErrors = JsonConvert.DeserializeObject<ValidationErrors>(errorContent);
                    if (validationErrors?.Errors != null && validationErrors.Errors.Count > 0)
                    {
                        ListValidationErrors.AddRange(validationErrors.Errors);
                    }
                    else
                    {
                        ListValidationErrors.Add("Errore generico dal server");
                    }
                }
                catch
                {
                    ListValidationErrors.Add("Email o password errati");
                }

                return Page();
            }

            ListValidationErrors.Add("Errore durante la connessione al server");
            return Page();
        }
    }

    public class TwoFactorRequiredResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; } = "";

        [JsonProperty("requires2FA")]
        public bool Requires2FA { get; set; } = false;

        [JsonProperty("userId")]  // Assicurati che il nome corrisponda a quello restituito dall'API
        public string UserId { get; set; } = "";
    }
}
