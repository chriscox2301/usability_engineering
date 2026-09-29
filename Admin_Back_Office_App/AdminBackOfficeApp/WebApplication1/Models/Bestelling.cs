namespace WebApplication1.Models
{
    /*
        Model: Bestelling

        Een Bestelling-object stelt één order voor.
        De klant-bestelapplicatie en de admin back-office gebruiken dezelfde SQL Server tabellen.
        Daardoor ziet de back-office dezelfde ordernummers, statussen, annuleringen en klachten
        die vanuit de klantapp worden aangemaakt.
    */
    public class Bestelling
    {
        public int Id { get; set; }
        public string Ordernummer { get; set; } = string.Empty;
        public string KlantNaam { get; set; } = string.Empty;
        public string Datum { get; set; } = string.Empty;
        public DateTime DatumTijd { get; set; }
        public string Email { get; set; } = string.Empty;
        public string VerzendAdres { get; set; } = string.Empty;
        public string OrderOpmerking { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Totaal { get; set; }
        public int AantalRegels { get; set; }
        public int AantalIngepakteRegels { get; set; }
        public string Prioriteit { get; set; } = "Normaal";
        public string PakketStatus { get; set; } = "Nog niet ingepakt";
        public int? BezorgerId { get; set; }
        public string BezorgerNaam { get; set; } = string.Empty;
        public List<Bestelregel> Regels { get; set; } = new();

        // True als elke orderregel door de orderpicker is aangevinkt.
        public bool AllesIngepakt => AantalRegels > 0 && AantalIngepakteRegels == AantalRegels;

        // Voortgangspercentage voor de horizontale pakketbezorging voortgangsbalk.
        public int BezorgProgressPercentage => Status switch
        {
            "Nieuw" => 15,
            "In behandeling" => 30,
            "Ingepakt" => 55,
            "Onderweg" => 85,
            "Bezorgd" => 100,
            "Annulering aangevraagd" => 25,
            "Geannuleerd" => 100,
            "Klacht gemeld" => 75,
            _ => 10
        };

        // Uitlegtekst onder de voortgangsbalk.
        public string BezorgProgressTekst => Status switch
        {
            "Nieuw" => "Bestelling ontvangen vanuit de klantapp.",
            "In behandeling" => "Back-office verwerkt de bestelling.",
            "Ingepakt" => "Alle producten zijn gecontroleerd en ingepakt.",
            "Onderweg" => string.IsNullOrWhiteSpace(BezorgerNaam)
                ? "Pakket is onderweg. Er is nog geen bezorger gekoppeld."
                : $"Pakket staat in de lijst van {BezorgerNaam}.",
            "Bezorgd" => "Pakket is geleverd bij de klant.",
            "Annulering aangevraagd" => "Klant heeft annulering aangevraagd. Back-office moet dit beoordelen.",
            "Geannuleerd" => "Bestelling is geannuleerd.",
            "Klacht gemeld" => "Klant heeft een klacht gemeld. Klantenservice moet dit opvolgen.",
            _ => "Status wordt verwerkt."
        };
    }
}
