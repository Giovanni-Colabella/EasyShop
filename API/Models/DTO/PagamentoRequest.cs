using API.Models.Entities;

namespace API.Models.DTO
{
    public record class PagamentoRequest
    {
        public List<ProdottoPaymentRequestDto> Prodotti { get; set; }
        public string Currency { get; init; } = "EUR";
        public decimal Amount { get; set; } = 0.00m;
        public string ReturnUrl { get; init; } = "http://localhost:5100/payment/paypal/paymentsuccess";
        public string CancelUrl { get; init; } = "http://localhost:5100/";

    }
}
