using API.Models.Services.Application;
using FluentValidation;

namespace API.Models.DTO.Validators;

public class OrdineDtoValidator : AbstractValidator<OrdineDto>
{
    private readonly IClienteService _clienteService;
    public OrdineDtoValidator(IClienteService clienteService)
    {
        _clienteService = clienteService;

        RuleFor(input => input.ClienteId)
            .NotEmpty().WithMessage("Il campo ClienteId è obbligatorio.")
            .GreaterThanOrEqualTo(1).WithMessage("Il campo ClienteId deve essere maggiore o uguale a 1")
            .Must(clienteId => ClienteIdEsiste(clienteId)).WithMessage("Il ClienteId specificato non esiste.");

        RuleFor(input => input.TotaleOrdine).NotEmpty().WithMessage("Il campo TotaleOrdine è obbligatorio.")
            .GreaterThanOrEqualTo(1).WithMessage("Il campo TotaleOrdine deve essere maggiore o uguale a 1")
            .LessThanOrEqualTo(10000).WithMessage("Il campo TotaleOrdine deve essere minore o uguale a 10000");

        RuleFor(input => input.TotaleOrdine)
            .NotEmpty().WithMessage("Il campo TotaleOrdine è obbligatorio.")
            .GreaterThanOrEqualTo(1).WithMessage("Il campo TotaleOrdine deve essere maggiore o uguale a 1.")
            .LessThanOrEqualTo(10000).WithMessage("Il campo TotaleOrdine deve essere minore o uguale a 10000.");


        RuleFor(input => input.DataOrdine).NotEmpty().WithMessage("Il campo DataOrdine è obbligatorio.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Il campo DataOrdine non può essere nel futuro.")
            .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("Il campo DataOrdine non può essere troppo nel passato");

        RuleFor(input => input.Stato).NotEmpty().WithMessage("Il campo Stato è obbligatorio.");
            // .Matches(@"^(In elaborazione|Spedito|Consegnato|Annullato)$").WithMessage("Lo stato dell'ordine deve essere 'In elaborazione', 'Spedito', 'Consegnato' o 'Annullato'");

        RuleFor(input => input.MetodoPagamento).NotEmpty().WithMessage("Il campo MetodoPagamento è obbligatorio.")
            .Matches(@"^(Carta di credito|PayPal|Bonifico bancario)$").WithMessage("Il metodo di pagamento deve essere 'Carta di credito', 'PayPal' o 'Bonifico bancario'");

    }

    private bool ClienteIdEsiste(int clienteId)
    {
        // Verifica se esiste un cliente con l'id specificato
        return _clienteService.ClienteExists(clienteId);
    }

}
