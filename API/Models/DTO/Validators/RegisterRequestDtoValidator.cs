using System;
using FluentValidation;

namespace API.Models.DTO.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(input => input.Nome)
            .NotEmpty().WithMessage("Il campo 'Nome' non può essere vuoto")
            .MaximumLength(20).WithMessage("Il campo 'Nome' non può essere più lungo di 20 caratteri")
            .MinimumLength(3).WithMessage("Il campo 'Nome' non può essere più corto di 3 caratteri")
            .Matches(@"^[a-zA-Z''-'\s]{1,40}$").WithMessage("Il campo 'Nome' può contenere solo lettere e spazi");

        RuleFor(input => input.Cognome)
            .NotEmpty().WithMessage("Il campo 'Cognome' non può essere vuoto")
            .MaximumLength(20).WithMessage("Il campo 'Cognome' non può essere più lungo di 20 caratteri")
            .MinimumLength(3).WithMessage("Il campo 'Cognome' non può essere più corto di 3 caratteri")
            .Matches(@"^[a-zA-Z''-'\s]{1,40}$").WithMessage("Il campo 'Cognome' può contenere solo lettere e spazi");

        RuleFor(input => input.Email)
            .NotEmpty().WithMessage("Il campo 'Email' è obbligatorio")
            .EmailAddress().WithMessage("Il campo 'Email' deve essere un indirizzo email valido");

        RuleFor(input => input.Password)
            .NotEmpty().WithMessage("Il campo 'Password' è obbligatorio")
            .MinimumLength(8).WithMessage("La password deve essere lunga almeno 8 caratteri")
            .Matches(@"[A-Z]").WithMessage("La password deve contenere almeno una lettera maiuscola")
            .Matches(@"[a-z]").WithMessage("La password deve contenere almeno una lettera minuscola")
            .Matches(@"[0-9]").WithMessage("La password deve contenere almeno un numero")
            .Matches(@"[\W_]").WithMessage("La password deve contenere almeno un carattere speciale (come @, #, $, etc.)");

    }
}
