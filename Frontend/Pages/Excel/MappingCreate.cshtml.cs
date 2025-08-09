using Frontend.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Excel
{
    public class MappingCreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        public ExcelMappingHeaderCreateModel Mapping { get; set; } = new();

        public MappingCreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public void OnGet()
        {
            Mapping.Details.Add(new ExcelImportMappingDetailModel());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var response = await _httpClient.PostAsJsonAsync("api/excel/create-mapping", Mapping);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Excel/MappingList");
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                var errorContent = await response.Content.ReadAsStringAsync();

                // Aggiungi un errore al ModelState per visualizzarlo in pagina
                ModelState.AddModelError("", $"Errore HTTP {statusCode}: {errorContent}");

                return Page();
            }
        }
    }
}
