namespace API.Models.DTO
{
    public class CapturePaymentRequest
    {
        public string Token { get; set; } = string.Empty;
        public int ProdottoId { get; set; }
        public int Quantita { get; set; }
        
    }
} 