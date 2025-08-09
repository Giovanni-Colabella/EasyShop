using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Payment.PayPal
{
    public class PayPalConfirmedModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PayPalConfirmedModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Payment/PayPal/PaymentError");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["jwtToken"]);

                var payload = new { Token = token };
                var response = await client.PostAsJsonAsync("api/payment/capture", payload);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Payment capture failed: {response.StatusCode}, {errorContent}");
                    return RedirectToPage("/Payment/PayPal/PaymentError");
                }

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                int ordineId = json.GetProperty("ordineId").GetInt32();

                return RedirectToPage("/Payment/PayPal/PaymentSuccess", new { orderId = ordineId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing payment: {ex.Message}");
                return RedirectToPage("/Payment/PayPal/PaymentError");
            }
        }

    }
}
