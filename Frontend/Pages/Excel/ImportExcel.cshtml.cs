using System.Net.Http.Headers;

using Frontend.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Excel;

public class ImportExcelModel : PageModel
{
    private readonly HttpClient _httpClient;

    public ImportExcelModel(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Api");
    }

    [BindProperty]
    public IFormFile? ExcelFile { get; set; }

    [BindProperty]
    public int SelectedMappingId { get; set; }
    [BindProperty]
    public char EntityType { get; set; }

    public List<MappingSummaryModel> AvailableMappings { get; set; } = new();

    public ImportResultModel? ImportResult { get; set; }

    public async Task OnGetAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<MappingSummaryModel>>("/api/excel/get-mappings");
        if (result != null)
            AvailableMappings = result;
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        await OnGetAsync(); // ricarica i mapping

        if (ExcelFile == null || SelectedMappingId == 0)
        {
            ModelState.AddModelError(string.Empty, "Seleziona un file e un mapping.");
            return Page();
        }

        using var content = new MultipartFormDataContent();
        using var stream = ExcelFile.OpenReadStream();
        content.Add(new StreamContent(stream)
        {
            Headers = { ContentType = new MediaTypeHeaderValue(ExcelFile.ContentType) }
        }, "file", ExcelFile.FileName);
        content.Add(new StringContent(SelectedMappingId.ToString()), "mappingHeaderId");
        content.Add(new StringContent(EntityType.ToString()), "entityType");  

        var response = await _httpClient.PostAsync("/api/Excel/upload-excel", content);
        if (response.IsSuccessStatusCode)
        {
            ImportResult = await response.Content.ReadFromJsonAsync<ImportResultModel>();
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Errore durante l'importazione.");
        }

        return Page();
    }

}
