using FluentValidation;

namespace API.Models.DTO.Validators;

public class BanUserDtoValidator : AbstractValidator<BanUserDto>
{
    public BanUserDtoValidator()
    {
        RuleFor(input => input.Email)
            .NotEmpty().WithMessage("Email è obbligatorio")
            .EmailAddress().WithMessage("Email non è valida");

        RuleFor(input => input.Motivazione)
            .NotEmpty().WithMessage("Motivazione è obbligatoria")
            .MinimumLength(10).WithMessage("Motivazione deve essere lunga almeno 10 caratteri");
    }
}
