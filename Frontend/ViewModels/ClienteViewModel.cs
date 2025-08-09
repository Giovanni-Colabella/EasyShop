using Frontend.ViewModels.ValueObjects;

public class ClienteViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Cognome { get; set; } = "";
    public string Email { get; set; } = "";
    public Indirizzo Indirizzo { get; set; } = new Indirizzo();
    public string NumeroTelefono { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime DataIscrizione { get; set; }


}
