using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace Frontend.Pages.Excel
{
    public class MappingEditModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        public ExcelMappingHeaderUpdateModel Mapping { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public MappingEditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var mappingFromApi = await _httpClient
                .GetFromJsonAsync<ExcelMappingHeaderUpdateModel>($"api/excel/get-mappings/{Id}");
            if (mappingFromApi == null)
                return NotFound();

            Mapping = mappingFromApi;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var response = await _httpClient
                .PutAsJsonAsync($"api/excel/update-mappings/{Id}", Mapping);
            if (response.IsSuccessStatusCode)
                return RedirectToPage("MappingList");

            ModelState.AddModelError(string.Empty, "Errore nell'aggiornamento del mapping");
            return Page();
        }
    }
}
