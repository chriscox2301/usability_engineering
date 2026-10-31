// JUNIOR-UITLEG: Bestand: KE03_INTDEV_SE_1_Base/Pages/Producten/Overzicht.cshtml.cs
// Dit is de C#-code achter Producten/Overzicht.cshtml.
// Use case: UC01 Producten bekijken.
// Relaties:
// - Gebruikt IProductRepository om Producten uit de DataAccessLayer op te halen.
// - Geeft de lijst Producten door aan de .cshtml pagina, zodat Razor de productkaarten kan tonen.

using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages.Producten
{
    public class OverzichtModel : PageModel
    {
        // Repository waarmee deze pagina producten ophaalt.
        // Door de interface te gebruiken blijft de pagina los van de database-implementatie.
        private readonly IProductRepository _productRepository;

        // Producten is de lijst die in Overzicht.cshtml met een foreach wordt getoond.
        public List<Product> Producten { get; set; } = new();

        // Categorieen en Merken vullen de filterblokken aan de linkerkant van de productenpagina.
        public List<string> Categorieen { get; set; } = new();
        public List<string> Merken { get; set; } = new();

        // Constructor: ASP.NET geeft automatisch een ProductRepository mee via dependency injection.
        public OverzichtModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // OnGet wordt uitgevoerd wanneer de bezoeker de productenpagina opent.
        // categorie komt uit de URL, bijvoorbeeld /Producten/Overzicht?categorie=Lagers.
        // zoekterm komt uit de zoekbalk in de header.
        public void OnGet(string? categorie, string? zoekterm)
        {
            // Stap 1: haal alle producten op uit de database.
            var producten = _productRepository.HaalAlleProductenOp().ToList();

            // Stap 2: maak filterlijsten. Deze zijn vooral voor de gebruikersinterface.
            // Distinct voorkomt dubbele categorieën/merken.
            Categorieen = producten.Select(p => p.Categorie).Distinct().OrderBy(c => c).ToList();
            Merken = producten.Select(p => p.Merk)
                              .Where(m => !string.IsNullOrWhiteSpace(m))
                              .Distinct()
                              .OrderBy(m => m)
                              .ToList();

            // Stap 3: werkende categorie-filter.
            // Alleen producten uit de gekozen categorie blijven over.
            if (!string.IsNullOrWhiteSpace(categorie))
            {
                producten = producten.Where(p => p.Categorie == categorie).ToList();
            }

            // Stap 4: werkende zoekfunctie.
            // De klant kan zoeken op naam, omschrijving of merk.
            if (!string.IsNullOrWhiteSpace(zoekterm))
            {
                producten = producten.Where(p => p.Naam.Contains(zoekterm, StringComparison.OrdinalIgnoreCase)
                                              || p.Omschrijving.Contains(zoekterm, StringComparison.OrdinalIgnoreCase)
                                              || p.Merk.Contains(zoekterm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Stap 5: zet het resultaat klaar voor de Razor Page.
            Producten = producten;
        }

        // OnPostToevoegen wordt uitgevoerd wanneer de klant op de productenpagina direct op
        // "In winkelmand" klikt. Dit hoort bij UC02: Product toevoegen aan winkelmand.
        // Relatie met Product: we zoeken het Product op via productId.
        // Relatie met WinkelmandItem: we maken van het Product een tijdelijk WinkelmandItem.
        // Relatie met Session: de winkelmand wordt tijdelijk opgeslagen zolang de klant de site gebruikt.
        public IActionResult OnPostToevoegen(int productId, int aantal)
        {
            // Stap 1: haal het gekozen product op uit de DataAccessLayer.
            // We vertrouwen niet op naam/prijs uit HTML, omdat een gebruiker HTML kan aanpassen.
            var product = _productRepository.HaalProductOpMetId(productId);

			// Stap 2: als het product niet bestaat, sturen we de klant terug naar het overzicht.
			if (product == null)
			{
				return RedirectToPage("/Producten/Overzicht");
			}

			// Controleer of het product op voorraad is.
			if (product.Voorraad <= 0)
			{
				TempData["WinkelmandFoutmelding"] =
					$"Het product '{product.Naam}' is momenteel niet op voorraad en kan daarom niet aan je winkelmandje worden toegevoegd.";

				return RedirectToPage("/Producten/Overzicht");
			}

			// Stap 3: eenvoudige controle op het aantal.
			// Er is bewust geen maximum ingesteld, maar 0 of negatief mag niet.
			if (aantal < 1)
			{
				aantal = 1;
			}

			// Stap 4: haal de huidige winkelmand op uit Session.
			// Als er nog geen winkelmand is, maken we een lege lijst aan
			var winkelmand = HttpContext.Session.HaalObject<List<WinkelmandItem>>("Winkelmand")
							?? new List<WinkelmandItem>();

			// Stap 5: kijk of dit product al in de winkelmand staat.
			// Als dat zo is, verhogen we het aantal in plaats van een dubbele regel te maken.
			var bestaandItem = winkelmand.FirstOrDefault(item => item.ProductId == product.Id);

			// Controleer of het totaal aantal in de winkelmand niet groter is dan de voorraad.
			int huidigAantal = bestaandItem?.Aantal ?? 0;

			if (huidigAantal + aantal > product.Voorraad)
			{
				TempData["WinkelmandFoutmelding"] =
					$"Er zijn maar {product.Voorraad} stuks van '{product.Naam}' op voorraad.";

				return RedirectToPage("/Producten/Overzicht");
			}

            if (bestaandItem == null)
            {
                // Product staat nog niet in de winkelmand: maak een nieuwe winkelmandregel.
                winkelmand.Add(new WinkelmandItem
                {
                    ProductId = product.Id,
                    ProductNaam = product.Naam,
                    Omschrijving = product.Omschrijving,
                    Prijs = product.Prijs,
                    Artikelnummer = product.Artikelnummer,
                    Aantal = aantal,
                    Icoon = product.Icoon
                });
            }
            else
            {
                // Product staat al in de winkelmand: verhoog alleen het aantal.
                bestaandItem.Aantal += aantal;
            }

            // Stap 6: sla de aangepaste winkelmand terug op in Session.
            HttpContext.Session.ZetObject("Winkelmand", winkelmand);

            // Toon na de redirect een bevestiging in de gedeelde header.
            TempData["WinkelmandProductNaam"] = product.Naam;
            TempData["WinkelmandProductAantal"] = aantal;
            TempData["WinkelmandProductIcoon"] = product.Icoon;

            // Blijf op de productenpagina, zodat de klant verder kan winkelen.
            return RedirectToPage("/Producten/Overzicht");
        }

    }
}
