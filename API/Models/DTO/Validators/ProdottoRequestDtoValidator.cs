using FluentValidation;

namespace API.Models.DTO.Validators
{
    public class ProdottoRequestDtoValidator : AbstractValidator<ProdottoRequestDto>
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png" };
        private const int MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public ProdottoRequestDtoValidator()
        {
            RuleFor(p => p.NomeProdotto)
                .NotEmpty().WithMessage("Il nome del prodotto è obbligatorio.")
                .MinimumLength(3).WithMessage("Il nome del prodotto deve avere almeno 3 caratteri.")
                .MaximumLength(100).WithMessage("Il nome del prodotto non può superare i 100 caratteri.");

            RuleFor(p => p.Prezzo)
                .GreaterThan(0).WithMessage("Il prezzo deve essere maggiore di zero.");

            RuleFor(p => p.Categoria)
                .NotEmpty().WithMessage("La categoria è obbligatoria.");

            RuleFor(p => p.Descrizione)
                .MaximumLength(500).WithMessage("La descrizione non può superare i 500 caratteri.");

            RuleFor(p => p.Giacenza)
                .GreaterThanOrEqualTo(0).WithMessage("La giacenza non può essere negativa.");

            RuleFor(p => p.ImgFile)
                .Must(BeAValidImage).WithMessage("Il file deve essere un'immagine valida (.jpg, .jpeg, .png).")
                .Must(BeAValidMimeType).WithMessage("Il formato dell'immagine non è valido.")
                .Must(BeUnderMaxSize).WithMessage($"L'immagine non può superare i {MaxFileSize / 1024 / 1024} MB.");
            
        }

        private bool BeAValidImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            return _allowedExtensions.Contains(extension);
        }

        private bool BeAValidMimeType(IFormFile? file)
        {
            if (file == null) return true;

            return _allowedMimeTypes.Contains(file.ContentType);
        }

        private bool BeUnderMaxSize(IFormFile? file)
        {
            if (file == null) return true;

            return file.Length <= MaxFileSize;
        }
    }
}
