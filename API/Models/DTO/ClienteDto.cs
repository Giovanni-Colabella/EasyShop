using API.Models.ValueObjects;

namespace API.Models;

public record class ClienteDto{
    public int Id { get; init; } = 0;
    public string Nome { get; init; } = "";
    public string Cognome { get; init; } = "";
    public string Email { get; init; } = "";
    public Indirizzo Indirizzo { get; set; } = new Indirizzo();
    public string NumeroTelefono { get; init; } = "";
    public string Status { get; init; } = "Attivo";
    public DateTime DataIscrizione { get; init; } = DateTime.Now;
};

