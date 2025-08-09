namespace Frontend.ViewModels.ValueObjects;

public class Indirizzo
{
    public string Via { get; set; }
    public string Citta { get; set; }
    public string CAP { get; set; }

    public Indirizzo()
        : this("", "", "")
    {

    }

    public Indirizzo(string via, string citta, string cap)
    {
        Via = via;
        Citta = citta;
        CAP = cap;
    }
}