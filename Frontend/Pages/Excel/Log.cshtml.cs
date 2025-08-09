using Frontend.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Excel
{
    public class LogModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<ImportErrorPreviewModel> LogDetails { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public LogModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id <= 0)
                return BadRequest();

            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ImportErrorPreviewModel>>($"api/excel/errors/{Id}");
                if (result == null)
                    return NotFound();

                LogDetails = result;
                return Page();
            }
            catch (HttpRequestException)
            {
                return StatusCode(503); 
            }
        }
    }
}
