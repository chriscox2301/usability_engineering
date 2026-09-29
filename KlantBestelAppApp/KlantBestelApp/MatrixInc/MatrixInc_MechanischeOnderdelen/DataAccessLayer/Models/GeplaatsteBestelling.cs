// JUNIOR-UITLEG: Bestand: DataAccessLayer/Models/GeplaatsteBestelling.cs
// Deze klasse wordt gebruikt voor de orderhistorie die uit SQL Server wordt gelezen.
// Relaties:
// - GeplaatsteBestelling bevat meerdere Bestelregels.
// - GeplaatsteBestelling ontstaat bij het plaatsen van een bestelling en wordt opgeslagen in SQL Server.
// - Extra velden zoals BezorgMelding, AnnuleringAangevraagd en KlachtAangemaakt maken de orderhistorie interactiever.

namespace DataAccessLayer.Models
{
    public class GeplaatsteBestelling
    {
        // Id van de bestelling uit SQL Server.
        public int Id { get; set; }

        // Ordernummer wordt gegenereerd tijdens bestellen, bijvoorbeeld MI-20260524143000.
        public string Ordernummer { get; set; } = string.Empty;

        // Datum waarop de klant de bestelling geplaatst heeft.
        public DateTime Besteldatum { get; set; }

        // Status van de bestelling, bijvoorbeeld Geplaatst, Onderweg, Bezorgd of Geannuleerd.
        public string Status { get; set; } = "Geplaatst";

        // Totaalbedrag is het bedrag van alle regels samen.
        public decimal Totaalbedrag { get; set; }

        // Simpele klantgegevens uit het checkoutformulier.
        public string KlantNaam { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VerzendAdres { get; set; } = string.Empty;

        // Optionele opmerkingen die de klant tijdens het bestellen invult.
        public string OrderOpmerkingen { get; set; } = string.Empty;

        // Extra melding vanuit bezorging/back-office.
        // Hiermee kan de klant bijvoorbeeld zien dat een bezorger een probleem heeft gemeld.
        public string BezorgMelding { get; set; } = string.Empty;

        // Pakketstatus uit de back-office, bijvoorbeeld Nog niet ingepakt, Bij bezorger of Bezorgd.
        public string PakketStatus { get; set; } = "Nog niet ingepakt";

        // Naam van de bezorger als back-office er één heeft gekoppeld.
        public string BezorgerNaam { get; set; } = string.Empty;

        // Deze booleans zijn bedoeld om acties in de orderhistorie zichtbaar te maken.
        public bool AnnuleringAangevraagd { get; set; }
        public bool KlachtAangemaakt { get; set; }


        // Percentage voor de horizontale voortgangsbalk in de orderhistorie.
        // Deze gebruikt dezelfde statuslogica als de admin back-office.
        public int BezorgProgressPercentage => Status switch
        {
            "Nieuw" => 15,
            "Geplaatst" => 15,
            "In behandeling" => 30,
            "Verwerkt" => 30,
            "Ingepakt" => 50,
            "Verzonden" => 70,
            "Onderweg" => 85,
            "Bezorgd" => 100,
            "Annulering aangevraagd" => 25,
            "Geannuleerd" => 100,
            "Klacht gemeld" => 75,
            _ => 10
        };

        // Uitlegtekst onder de voortgangsbalk.
        // Hierdoor ziet de klant in gewone taal wat de pakketstatus betekent.
        public string BezorgProgressTekst => Status switch
        {
            "Nieuw" => "Bestelling ontvangen vanuit de klantapp.",
            "Geplaatst" => "Bestelling ontvangen vanuit de klantapp.",
            "In behandeling" => "Back-office verwerkt de bestelling.",
            "Verwerkt" => "Back-office verwerkt de bestelling.",
            "Ingepakt" => "Alle producten zijn gecontroleerd en ingepakt.",
            "Verzonden" => "Pakket is overgedragen aan bezorging.",
            "Onderweg" => string.IsNullOrWhiteSpace(BezorgerNaam)
                ? "Pakket is onderweg. Er is nog geen bezorger gekoppeld."
                : $"Pakket staat in de lijst van {BezorgerNaam}.",
            "Bezorgd" => "Pakket is geleverd bij de klant.",
            "Annulering aangevraagd" => "Je annulering is aangevraagd. Back-office beoordeelt dit.",
            "Geannuleerd" => "Deze bestelling is geannuleerd.",
            "Klacht gemeld" => "Je klacht is gemeld. Klantenservice moet dit opvolgen.",
            _ => "Status wordt verwerkt."
        };

        // Relatie met Bestelregel:
        // Een bestelling moet minimaal één productregel kunnen tonen.
        public List<Bestelregel> Regels { get; set; } = new List<Bestelregel>();
    }
}
