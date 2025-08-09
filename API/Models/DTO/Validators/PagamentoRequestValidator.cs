using FluentValidation;

namespace API.Models.DTO.Validators
{
    public class PagamentoRequestValidator : AbstractValidator<PagamentoRequest>
    {
        public PagamentoRequestValidator()
        {
            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("La valuta è obbligatoria.")
                .Length(3).WithMessage("Il codice valuta deve avere 3 caratteri (es. EUR, USD).");

            RuleFor(x => x.Prodotti)
                .NotEmpty().WithMessage("Deve essere presente almeno un prodotto nel pagamento.");

            //RuleFor(x => x.ClienteId)
            //    .NotEmpty().WithMessage("Il clienteId non può essere vuoto");
        }
    }

   
}
