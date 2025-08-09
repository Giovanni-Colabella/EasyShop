using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Frontend.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RegisterModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("Api"); // client configurato con base URL
            _configuration = configuration;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        public class RegisterInputModel
        {
            [Display(Name = "Nome")]
            public string Nome { get; set; } = "";

            [Display(Name = "Cognome")]
            public string Cognome { get; set; } = "";

            [Display(Name = "Email")]
            public string Email { get; set; } = "";

            [Display(Name = "Password")]
            public string Password { get; set; } = "";
        }

        public List<string> ListValidationErrors { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Sicurezza: evitare valori null
            Input.Nome ??= "";
            Input.Cognome ??= "";
            Input.Email ??= "";
            Input.Password ??= "";

            var requestData = new
            {
                Input.Nome,
                Input.Cognome,
                Input.Email,
                Input.Password
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(requestData),
                Encoding.UTF8,
                "application/json"
            );

            // Usa endpoint relativo, base url è già configurata
            var response = await _httpClient.PostAsync("api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Auth/EmailVerificationSent");
            }

            var errorContent = await response.Content.ReadAsStringAsync();

            try
            {
                var validationErrors = JsonConvert.DeserializeObject<ValidationErrors>(errorContent);

                if (validationErrors?.Errors != null && validationErrors.Errors.Count > 0)
                {
                    foreach (var errorMessage in validationErrors.Errors)
                    {
                        ListValidationErrors.Add(errorMessage);
                        ModelState.AddModelError(string.Empty, errorMessage);
                    }
                }
                else
                {
                    var genericError = "Errore generico del server";
                    ModelState.AddModelError(string.Empty, genericError);
                    ListValidationErrors.Add(genericError);
                }
            }
            catch (JsonException)
            {
                var parseError = "Errore nel formato della risposta dal server";
                ListValidationErrors.Add(parseError);
                ModelState.AddModelError(string.Empty, parseError);
            }

            return Page();
        }
    }

    // Classe per mappare il JSON degli errori
    public class ValidationErrors
    {
        [JsonProperty("errors")]
        public List<string> Errors { get; set; } = new List<string>();
    }
}
