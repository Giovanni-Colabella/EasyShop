using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Payment.PayPal
{
    public class PaymentSuccess : PageModel
    {
        private readonly ILogger<PaymentSuccess> _logger;
        public int OrdineId { get; set; }

        public PaymentSuccess(ILogger<PaymentSuccess> logger)
        {
            _logger = logger;
        }

        public void OnGet(int orderId)
        {
            OrdineId = orderId;
        }
    }
}