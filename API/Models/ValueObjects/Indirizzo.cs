namespace API.Models.ValueObjects
{
    public class Indirizzo
    {
        public string Via { get; set; }
        public string Citta { get; set; }
        public string CAP { get; set; }
        public string HouseNumber {get; set; }

        public Indirizzo()
            :this("", "", "", "")
        {

        }

        public Indirizzo(string via, string citta, string cap, string housenumber)
        {
            Via = via;
            Citta = citta;
            CAP = cap;
            HouseNumber = housenumber;
        }
    }
}
