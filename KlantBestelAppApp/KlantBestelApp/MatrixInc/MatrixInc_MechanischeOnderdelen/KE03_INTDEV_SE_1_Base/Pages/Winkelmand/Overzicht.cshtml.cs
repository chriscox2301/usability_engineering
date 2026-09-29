// JUNIOR-UITLEG: Bestand: KE03_INTDEV_SE_1_Base/Pages/Winkelmand/Overzicht.cshtml.cs
// Dit is de C#-code achter Winkelmand/Overzicht.cshtml.
// Use case: onderdeel van UC02 Product toevoegen aan winkelmand en UC03 Bestelling plaatsen.
// Relaties:
// - Leest List<WinkelmandItem> uit Session.
// - Toont totaalbedrag op de winkelmandpagina.
// - Kan een WinkelmandItem verwijderen uit Session.

using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages.Winkelmand
{
    public class OverzichtModel : PageModel
    {
        // De sleutel waarmee de winkelmand in Session wordt opgeslagen.
        private const string WinkelmandSleutel = "Winkelmand";

        // Items is de lijst met producten in de winkelmand.
        public List<WinkelmandItem> Items { get; set; } = new();

        // Totaal telt alle subtotalen bij elkaar op.
        // Dit is een berekende property, dus er is geen aparte methode nodig.
        public decimal Totaal => Items.Sum(i => i.Subtotaal);

        // OnGet wordt uitgevoerd wanneer de winkelmandpagina wordt geopend.
        public void OnGet()
        {
            // Haal de winkelmand uit Session.
            // Als Session nog leeg is, gebruiken we een lege lijst.
            Items = HttpContext.Session.HaalObject<List<WinkelmandItem>>(WinkelmandSleutel) ?? new List<WinkelmandItem>();
        }


        // Deze handler wordt uitgevoerd door het formulier met asp-page-handler="AantalAanpassen".
        // Relatie met WinkelmandItem:
        // - productId vertelt welk WinkelmandItem aangepast moet worden.
        // - aantal is het nieuwe aantal dat de klant in de winkelmand heeft ingevuld.
        //
        // Waarom staat deze functie op de winkelmandpagina?
        // De winkelmand is de plek waar de klant zijn gekozen producten controleert.
        // Daarom moet de klant hier ook het aantal kunnen wijzigen voordat hij de bestelling plaatst.
        public IActionResult OnPostAantalAanpassen(int productId, int aantal)
        {
            // Haal huidige winkelmand op uit Session.
            // Session is tijdelijke opslag voor deze gebruiker tijdens het bezoeken van de website.
            var items = HttpContext.Session.HaalObject<List<WinkelmandItem>>(WinkelmandSleutel) ?? new List<WinkelmandItem>();

            // Zoek het winkelmanditem dat hoort bij het productId.
            var item = items.FirstOrDefault(i => i.ProductId == productId);

            // Alleen aanpassen als het product echt in de winkelmand staat.
            if (item != null)
            {
                // Een klant moet minimaal 1 stuk bestellen.
                // Als iemand per ongeluk 0 of een negatief getal invult, zetten we het terug naar 1.
                // Er is geen maximum ingesteld, zodat iemand in één keer veel producten kan bestellen.
                item.Aantal = aantal < 1 ? 1 : aantal;
            }

            // Sla de aangepaste winkelmand weer op in Session.
            HttpContext.Session.ZetObject(WinkelmandSleutel, items);

            // Herlaad de pagina zodat het nieuwe subtotaal en totaalbedrag zichtbaar worden.
            return RedirectToPage();
        }

        // Deze handler wordt uitgevoerd door het formulier met asp-page-handler="Verwijderen".
        // productId vertelt welk product uit de winkelmand moet worden verwijderd.
        public IActionResult OnPostVerwijderen(int productId)
        {
            // Haal huidige winkelmand op.
            var items = HttpContext.Session.HaalObject<List<WinkelmandItem>>(WinkelmandSleutel) ?? new List<WinkelmandItem>();

            // Zoek het item dat overeenkomt met het productId.
            var item = items.FirstOrDefault(i => i.ProductId == productId);

            // Als het item bestaat, verwijderen we het uit de lijst.
            if (item != null)
            {
                items.Remove(item);
            }

            // Sla de aangepaste lijst weer op in Session.
            HttpContext.Session.ZetObject(WinkelmandSleutel, items);

            // Herlaad de winkelmandpagina zodat de klant het nieuwe overzicht ziet.
            return RedirectToPage();
        }
    }
}
