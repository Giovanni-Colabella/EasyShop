using Frontend.ViewModels;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Excel
{
    public class LogsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<ExcelLogSummaryViewModel> Logs { get; set; } = new();

        public LogsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task OnGetAsync()
        {
            try
            {

                var result = await _httpClient.GetFromJsonAsync<List<ExcelLogSummaryViewModel>>("api/excel/errors");
                if (result != null)
                {
                    Logs = result;
                }
            }
            catch (HttpRequestException)
            {
                Logs = new List<ExcelLogSummaryViewModel>();
            }
        }
    }
}
