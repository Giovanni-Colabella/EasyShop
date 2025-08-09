using FluentValidation;

namespace API.Models.DTO.Validators;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(input => input.Email)
            .NotEmpty().WithMessage("Il campo 'Email' non può essere vuoto")
            .EmailAddress().WithMessage("Il campo 'Email' deve essere un indirizzo email valido");

        RuleFor(input => input.Password)
            .NotEmpty().WithMessage("Il campo 'Password' è obbligatorio");

    }
}
