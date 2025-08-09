using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace Frontend.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public LogoutModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PostAsync("api/auth/logout", null);

            // Cancelliamo il cookie JWT lato client dopo logout
            Response.Cookies.Delete("jwtToken");

            // Puoi eventualmente gestire l'eventuale fallimento logout
            // ma in ogni caso facciamo redirect

            return RedirectToPage("/Index");
        }
    }
}
