using API.Models.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace API.Models.DTO.Validators
{
    public class UpdatePasswordRequestDtoValidator : AbstractValidator<UpdatePasswordRequestDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdatePasswordRequestDtoValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("ID utente obbligatorio");


            RuleFor(x => x.PasswordCorrente)
                .NotEmpty().WithMessage("Password corrente obbligatoria")
                .MustAsync(BeCurrentPasswordValid).WithMessage("Password corrente non valida");

            RuleFor(x => x.NuovaPassword)
                .NotEmpty().WithMessage("Nuova password obbligatoria")
                .MinimumLength(8).WithMessage("Minimo 8 caratteri")
                .MaximumLength(20).WithMessage("Massimo 20 caratteri")
                .Matches("[A-Z]").WithMessage("Richiesta almeno una maiuscola")
                .Matches("[a-z]").WithMessage("Richiesta almeno una minuscola")
                .Matches("[0-9]").WithMessage("Richiesto almeno un numero")
                .Matches(@"[\W_]").WithMessage("Richiesto un carattere speciale")
                .NotEqual(x => x.PasswordCorrente).WithMessage("La nuova password deve essere diversa dalla corrente");

   
            RuleFor(x => x.ConfermaPassword)
                .NotEmpty().WithMessage("Conferma password obbligatoria")
                .Equal(x => x.NuovaPassword).WithMessage("Le password non coincidono");
        }

        private async Task<bool> BeCurrentPasswordValid(UpdatePasswordRequestDto dto, string currentPassword, CancellationToken token)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            return user != null && await _userManager.CheckPasswordAsync(user, currentPassword);
        }
    }
}