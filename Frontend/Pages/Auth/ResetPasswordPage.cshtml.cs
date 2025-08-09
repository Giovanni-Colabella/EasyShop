using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Auth;

public class ResetPasswordPageModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public ResetPasswordPageModel(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _config = config;
        _httpClient = httpClientFactory.CreateClient("Api"); 
    }

    [BindProperty]
    public string UserId { get; set; } = "";

    [BindProperty]
    public string Token { get; set; } = "";

    [BindProperty]
    public string NuovaPassword { get; set; } = "";

    public string Message { get; set; } = "";

    public void OnGet(string userid, string token)
    {
        UserId = userid;
        Token = token;
    }

    public async Task<IActionResult> OnPostSubmit()
    {
        var requestData = new
        {
            UserId,
            Token,
            NuovaPassword
        };

        var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", requestData);

        if (response.IsSuccessStatusCode)
        {
            Message = "Password reimpostata.";
        }
        else
        {
            Message = "Errore durante il reset.";
        }

        return Page();
    }
}
