namespace Frontend.ViewModels
{
    public class PagamentoViewModel
    {
        public List<ProdottoViewModel> Prodotti { get; set; } = new();
        public string Currency { get; init; } = "EUR";
        public decimal Amount { get; set; } = 0.00m;
        public string ReturnUrl { get; init; } = string.Empty;
        public string CancelUrl { get; init; } = string.Empty;
    }
}
