using System;
using System.ComponentModel.DataAnnotations;

namespace API.Models.DTO;

public class ProdottoPaymentRequestDto
{
    [Required]
    public int Id { get; set; }

    public int Quantita { get; set; } = 1;
}
