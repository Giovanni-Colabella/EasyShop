using Bogus;

namespace API.Models.ValueObjects
{
    public class GeneratoreDati
    {
        public static List<ClienteGenerato> GeneraClienti(int count)
        {
            var faker = new Faker<ClienteGenerato>("it")
                .RuleFor(c => c.Nome, f => f.Name.FirstName())
                .RuleFor(c => c.Cognome, f => f.Name.LastName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.NumeroTelefono, f => f.Phone.PhoneNumber("##########"))
                .RuleFor(c => c.Indirizzo, f => new Indirizzo{
                    Via = f.Address.StreetName(),
                    Citta = f.Address.City(),
                    CAP = f.Address.ZipCode()
                })
                .RuleFor(c => c.Status, f => f.PickRandom("Attivo", "Inattivo"))  
                .RuleFor(u => u.DataIscrizione, f => f.Date.Past(5));

            return faker.Generate(count);
        }
    }

    public class ClienteGenerato
    {
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Email { get; set; }  
        public string NumeroTelefono { get; set; }      
        public Indirizzo Indirizzo {get; set; }
        public string Status { get; set; }
        public DateTime DataIscrizione { get; set; }
    }
}