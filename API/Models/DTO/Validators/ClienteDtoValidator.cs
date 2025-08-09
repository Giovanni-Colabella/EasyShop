using FluentValidation;

namespace API.Models.Requests.Validators;

public class ClienteDtoValidator : AbstractValidator<ClienteDto>
{
    public ClienteDtoValidator()
    {
        RuleFor(input => input.Nome)
            .NotEmpty().WithMessage("Il campo 'Nome' è obbligatorio")
            .MaximumLength(20).WithMessage("Il campo 'Nome' deve essere lungo al massimo 50 caratteri")
            .MinimumLength(3).WithMessage("Il campo 'Nome' deve essere lungo al minimo 3 caratteri")
            .Matches(@"^[a-zA-Z''-'\s]{1,40}$").WithMessage("Il campo 'Nome' può contenere solo lettere e spazi");

        RuleFor(input => input.Cognome)
            .NotEmpty().WithMessage("Il campo 'Cognome' è obbligatorio")
            .MaximumLength(20).WithMessage("Il campo 'Cognome' deve essere lungo al massimo 50 caratteri")
            .MinimumLength(3).WithMessage("Il campo 'Cognome' deve essere lungo al minimo 3 caratteri")
            .Matches(@"^[a-zA-Z''-'\s]{1,40}$").WithMessage("Il campo 'Cognome' può contenere solo lettere e spazi");
        
        RuleFor(input => input.Email)
            .NotEmpty().WithMessage("Il campo 'Email' è obbligatorio")
            .EmailAddress().WithMessage("Il campo 'Email' deve essere un indirizzo email valido");
        

        RuleFor(input => input.NumeroTelefono)
            .NotEmpty().WithMessage("Il campo 'NumeroTelefono' è obbligatorio")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Il campo 'NumeroTelefono' deve essere un numero di telefono valido");

        RuleFor(input => input.Indirizzo.Via)
            .NotEmpty().WithMessage("Il campo 'Via' è obbligatorio");
        
        RuleFor(input => input.Indirizzo.Citta)
            .NotEmpty().WithMessage("Il campo 'Città' è obbligatorio");
        
        RuleFor(input => input.Indirizzo.CAP)
            .NotEmpty().WithMessage("Il campo 'CAP' è obbligatorio");
        

        RuleFor(input => input.Status)
            .NotEmpty().WithMessage("Il campo 'Status' è obbligatorio")
            .Matches(@"^(Attivo|Inattivo)$")
            .WithMessage("Lo status del cliente deve essere 'Attivo' o 'Inattivo'");

        RuleFor(input => input.DataIscrizione)
            .NotEmpty().WithMessage("Il campo 'DataIscrizione' è obbligatorio")
            .LessThan(DateTime.Now)
            .WithMessage("La data di iscrizione non può essere nel futuro")
            .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("La data di iscrizione non può essere troppo indietro nel tempo");
        
    }
}
