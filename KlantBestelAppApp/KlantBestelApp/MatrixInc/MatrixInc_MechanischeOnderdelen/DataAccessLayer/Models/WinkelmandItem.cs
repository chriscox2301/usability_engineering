// JUNIOR-UITLEG: Bestand: DataAccessLayer/Models/WinkelmandItem.cs
// Deze klasse stelt één regel in de winkelmand voor.
// Relaties:
// - WinkelmandItem verwijst via ProductId naar een Product.
// - De winkelmand zelf wordt opgeslagen in Session als List<WinkelmandItem>.
// - Bij bestelling plaatsen worden WinkelmandItems omgezet naar Bestelregels.

namespace DataAccessLayer.Models
{
    public class WinkelmandItem
    {
        // Id van het product dat in de winkelmand zit.
        // We slaan niet het hele Product-object op, maar alleen de gegevens die nodig zijn voor de winkelmand.
        public int ProductId { get; set; }

        // Naam, omschrijving, prijs en icoon worden gekopieerd uit Product.
        // Zo blijft de winkelmand makkelijk te tonen.
        public string ProductNaam { get; set; } = string.Empty;
        public string Omschrijving { get; set; } = string.Empty;
        public string Artikelnummer { get; set; } = string.Empty;
        public decimal Prijs { get; set; }

        // Aantal geeft aan hoeveel stuks de klant wil bestellen.
        // In de productdetailpagina is geen maximum ingesteld, zodat iemand veel producten tegelijk kan bestellen.
        public int Aantal { get; set; }

        // Simpel icoon voor de productkaart in de winkelmand.
        public string Icoon { get; set; } = "cog";

        // Gedrag / berekende property:
        // Subtotaal is geen los opgeslagen veld, maar wordt automatisch berekend uit Prijs x Aantal.
        // Voorbeeld: €10 x 3 = €30.
        public decimal Subtotaal => Prijs * Aantal;
    }
}
