using Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Excel
{
    public class MappingListModel : PageModel
    {
        private readonly HttpClient _httpClient;
        public List<MappingSummaryModel> Mappings { get; set; } = new();

        public MappingListModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task OnGetAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<List<MappingSummaryModel>>("api/excel/get-mappings");
            if (result != null)
                Mappings = result;
        }
    }
}
