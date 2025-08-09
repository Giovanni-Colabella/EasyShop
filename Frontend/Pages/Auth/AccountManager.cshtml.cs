using System.Net.Http.Headers;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frontend.Pages.Auth
{
    public class AccountManagerModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public AccountManagerModel(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _client = clientFactory.CreateClient("Api");
            _config = config;
            ListValidationErrors = new List<string>();
            ListPasswordValidationErrors = new List<string>();
        }

        public ApplicationUserViewModel User { get; set; } = new();

        [BindProperty]
        public ApplicationUserInputModel UserInputModel { get; set; } = new();

        [BindProperty]
        public PasswordInputModel Password { get; set; } = new();

        public List<string> ListValidationErrors { get; set; }
        public List<string> ListPasswordValidationErrors { get; set; }

        public bool SuccessProfileEdited { get; set; } = false;
        public bool SuccessPasswordEdited { get; set; } = false;
        public bool Is2FAEnabled { get; set; } = false;
        public string IndirizzoPlaceholder { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync("api/auth/GetUser");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                User = JsonConvert.DeserializeObject<ApplicationUserViewModel>(json) ?? new ApplicationUserViewModel();

                Is2FAEnabled = User.Is2FAEnabled;

                UserInputModel = new ApplicationUserInputModel
                {
                    UserId = User.UserId,
                    Nome = User.Nome,
                    Cognome = User.Cognome,
                    Email = User.Email
                };
            }

            if (!response.IsSuccessStatusCode)
                return Page();

            var responseAddress = await _client.GetAsync("api/address/get-user-address");

            if (responseAddress.IsSuccessStatusCode)
            {
                var content = await responseAddress.Content.ReadAsStringAsync();
                var jsonAddress = JObject.Parse(content);

                UserInputModel.Indirizzo_Citta = (string?)jsonAddress["indirizzo_Citta"] ?? string.Empty;
                UserInputModel.Indirizzo_Via = (string?)jsonAddress["indirizzo_Via"] ?? string.Empty;
                UserInputModel.Indirizzo_CAP = (string?)jsonAddress["indirizzo_CAP"] ?? string.Empty;
                UserInputModel.Indirizzo_HouseNumber = (string?)jsonAddress["indirizzo_HouseNumber"] ?? string.Empty;

                IndirizzoPlaceholder =
                    $"{UserInputModel.Indirizzo_Via}, " +
                    $"{UserInputModel.Indirizzo_Citta} - " +
                    $"{UserInputModel.Indirizzo_CAP} || " +
                    $"Civico: {UserInputModel.Indirizzo_HouseNumber}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostProfiloAsync()
        {
            // assicurati che le proprietà non siano null
            UserInputModel.UserId ??= "";
            UserInputModel.Nome ??= "";
            UserInputModel.Cognome ??= "";
            UserInputModel.Email ??= "";
            UserInputModel.Indirizzo_Citta ??= "";
            UserInputModel.Indirizzo_CAP ??= "";
            UserInputModel.Indirizzo_Via ??= "";
            UserInputModel.Indirizzo_HouseNumber ??= "";

            SuccessProfileEdited = false;

            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PutAsJsonAsync("api/auth/updateAccount", UserInputModel);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                {
                    ListValidationErrors.Add("Errore del server: risposta vuota");
                    await ReloadUserData();
                    return Page();
                }

                try
                {
                    var validationErrors = JsonConvert.DeserializeObject<AccountValidationErrors>(content);
                    if (validationErrors?.Errors != null && validationErrors.Errors.Count > 0)
                    {
                        ListValidationErrors.AddRange(validationErrors.Errors);
                    }
                    else
                    {
                        ListValidationErrors.Add("Errore del server: nessun dettaglio sugli errori.");
                    }
                }
                catch (Exception)
                {
                    ListValidationErrors.Add("Errore durante la deserializzazione del JSON.");
                }

                await ReloadUserData();
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                ListValidationErrors.Add("Errore generico durante l'aggiornamento.");
                await ReloadUserData();
                return Page();
            }

            SuccessProfileEdited = true;
            await ReloadUserData();
            return Page();
        }

        public async Task<IActionResult> OnPostPasswordAsync()
        {
            Password.UserId ??= "";
            Password.PasswordCorrente ??= "";
            Password.NuovaPassword ??= "";
            Password.ConfermaPassword ??= "";

            SuccessPasswordEdited = false;

            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var request = new StringContent(JsonConvert.SerializeObject(Password), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/auth/updatepassword", request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(content))
                {
                    ListPasswordValidationErrors.Add("Errore del server: risposta vuota");
                    await ReloadUserData();
                    return Page();
                }

                try
                {
                    var validationErrors = JsonConvert.DeserializeObject<AccountValidationErrors>(content);

                    if (validationErrors?.Errors != null && validationErrors.Errors.Count > 0)
                    {
                        ListPasswordValidationErrors.AddRange(validationErrors.Errors);
                    }
                    else
                    {
                        ListPasswordValidationErrors.Add("Errore del server: nessun dettaglio sugli errori.");
                    }
                }
                catch
                {
                    ListPasswordValidationErrors.Add("Errore durante la deserializzazione del JSON.");
                }

                await ReloadUserData();
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                ListPasswordValidationErrors.Add("Errore generico durante l'aggiornamento.");
                await ReloadUserData();
                return Page();
            }

            SuccessPasswordEdited = true;
            await ReloadUserData();
            return Page();
        }

        private async Task ReloadUserData()
        {
            try
            {
                var token = Request.Cookies["jwtToken"];

                if (string.IsNullOrEmpty(token))
                {
                    RedirectToPage("/Auth/Login");
                    return;
                }

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _client.GetAsync("api/auth/GetUser");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    User = JsonConvert.DeserializeObject<ApplicationUserViewModel>(json) ?? new ApplicationUserViewModel();

                    Is2FAEnabled = User.Is2FAEnabled;

                    UserInputModel = new ApplicationUserInputModel
                    {
                        UserId = User.UserId ?? "",
                        Nome = User.Nome ?? "",
                        Cognome = User.Cognome ?? "",
                        Email = User.Email ?? ""
                    };
                }

                var responseAddress = await _client.GetAsync("api/address/get-user-address");

                if (responseAddress.IsSuccessStatusCode)
                {
                    var content = await responseAddress.Content.ReadAsStringAsync();
                    var jsonAddress = JObject.Parse(content);

                    UserInputModel.Indirizzo_Citta = (string?)jsonAddress["indirizzo_Citta"] ?? string.Empty;
                    UserInputModel.Indirizzo_Via = (string?)jsonAddress["indirizzo_Via"] ?? string.Empty;
                    UserInputModel.Indirizzo_CAP = (string?)jsonAddress["indirizzo_CAP"] ?? string.Empty;
                    UserInputModel.Indirizzo_HouseNumber = (string?)jsonAddress["indirizzo_HouseNumber"] ?? string.Empty;

                    IndirizzoPlaceholder =
                        $"{UserInputModel.Indirizzo_Via}, " +
                        $"{UserInputModel.Indirizzo_Citta} - " +
                        $"{UserInputModel.Indirizzo_CAP} || " +
                        $"Civico: {UserInputModel.Indirizzo_HouseNumber}";
                }
            }
            catch (Exception ex)
            {
                ListPasswordValidationErrors.Add("Errore durante il caricamento dei dati utente: " + ex.Message);
            }
        }

        public async Task<IActionResult> OnPostEnable2FAAsync()
        {
            return RedirectToPage("/Auth/QrCodePage");
        }

        public async Task<IActionResult> OnPostDisable2FAAsync()
        {
            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsync("api/auth/disable-2fa", null);

            await ReloadUserData();
            return Page();
        }
    }

    public class ApplicationUserViewModel
    {
        public string UserId { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Cognome { get; set; } = "";
        public string Email { get; set; } = "";
        public bool Is2FAEnabled { get; set; }
    }

    public class ApplicationUserInputModel
    {
        public string UserId { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Cognome { get; set; } = "";
        public string Email { get; set; } = "";
        public string Indirizzo_Citta { get; set; } = "";
        public string Indirizzo_Via { get; set; } = "";
        public string Indirizzo_CAP { get; set; } = "";
        public string Indirizzo_HouseNumber { get; set; } = "";
    }

    public class PasswordInputModel
    {
        public string UserId { get; set; } = "";
        public string PasswordCorrente { get; set; } = "";
        public string NuovaPassword { get; set; } = "";
        public string ConfermaPassword { get; set; } = "";
    }

    public class AccountValidationErrors
    {
        public List<string> Errors { get; set; } = new();
    }
}
