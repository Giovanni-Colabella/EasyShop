using System.ComponentModel.DataAnnotations;
using API.Models.Entities;
using API.Models.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index("Email", IsUnique = true)]
    public class Cliente
    {
        public int Id { get; set; } = 0;
        public string? UserId { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "il nome deve avere almeno un carattere.")]
        [MaxLength(20, ErrorMessage = "Il nome non può superare i 20 caratteri.")]
        [RegularExpression(@"^[\p{L}]+$", ErrorMessage = "Il campo Nome può contenere solo lettere.")]
        public string Nome { get; set; } = "";
        [Required]
        [MinLength(1, ErrorMessage = "il nome deve avere almeno un carattere.")]
        [MaxLength(20, ErrorMessage = "Il nome non può superare i 20 caratteri.")]
        [RegularExpression(@"^[\p{L}]+$", ErrorMessage = "Il campo Cognome può contenere solo lettere.")]
        public string Cognome { get; set; } = "";

        [Required(ErrorMessage = "L'email è obbligatoria.")]
        [EmailAddress(ErrorMessage = "L'email inserità non è un indirizzo valido")]
        public string Email { get; set; } = "";
        public string NumeroTelefono { get; set; } = "";
        public Indirizzo Indirizzo { get; set; } = new Indirizzo();
        public DateTime DataIscrizione { get; set; }
        

        public ApplicationUser? User { get; set; } 
        public List<Ordine> Ordini { get; set; } = new List<Ordine>();

    }

}
