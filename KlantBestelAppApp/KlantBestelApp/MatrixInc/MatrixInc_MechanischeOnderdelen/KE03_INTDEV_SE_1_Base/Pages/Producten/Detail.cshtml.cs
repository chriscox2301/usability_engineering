// JUNIOR-UITLEG: Bestand: KE03_INTDEV_SE_1_Base/Pages/Producten/Detail.cshtml.cs
// Dit is de C#-code achter Producten/Detail.cshtml.
// Use case: UC01 Product bekijken en UC02 Product toevoegen aan winkelmand.
// Relaties:
// - Haalt één Product op via IProductRepository.
// - Zet het gekozen product om naar een WinkelmandItem.
// - Slaat de winkelmand op in Session met SessieExtensions.

using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages.Producten
{
    public class DetailModel : PageModel
    {
        // Repository voor het ophalen van productgegevens.
        private readonly IProductRepository _productRepository;

        // Naam van de Session-sleutel.
        // Overal waar we de winkelmand opslaan/ophalen gebruiken we dezelfde tekst.
        private const string WinkelmandSleutel = "Winkelmand";

        // Product is het product dat op de detailpagina wordt getoond.
        // Het vraagteken betekent: Product kan null zijn als het Id niet bestaat.
        public Product? Product { get; set; }

        // ProductId komt uit het verborgen inputveld in het formulier.
        // BindProperty betekent: Razor Pages vult dit automatisch vanuit het formulier.
        [BindProperty]
        public int ProductId { get; set; }

        // Aantal komt uit het aantalveld op de detailpagina.
        // Er is bewust geen maximum, zodat een klant veel stuks in één keer kan bestellen.
        [BindProperty]
        public int Aantal { get; set; } = 1;

        // Constructor: ASP.NET geeft automatisch de productrepository mee.
        public DetailModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // OnGet wordt uitgevoerd wanneer de klant de detailpagina opent.
        // id komt uit de URL, bijvoorbeeld /Producten/Detail/3.
        public IActionResult OnGet(int id)
        {
            // Haal het product op dat bij dit id hoort.
            Product = _productRepository.HaalProductOpMetId(id);

            // Als het product bestaat, vullen we ProductId voor het formulier.
            if (Product != null)
            {
                ProductId = Product.Id;
            }

            // Toon de pagina.
            return Page();
        }

        // OnPost wordt uitgevoerd wanneer de klant op "Toevoegen aan winkelmand" klikt.
        public IActionResult OnPost()
        {
            // Stap 1: haal opnieuw het product op. Vertrouw niet blind op gegevens uit het formulier.
            var product = _productRepository.HaalProductOpMetId(ProductId);

            // Als het product niet bestaat, stuur de klant terug naar het overzicht.
            if (product == null)
            {
                return RedirectToPage("/Producten/Overzicht");
            }

            // Stap 2: eenvoudige validatie.
            // Als iemand 0 of negatief invult, maken we er 1 van.
            if (Aantal < 1)
            {
                Aantal = 1;
            }

            // Stap 3: haal huidige winkelmand uit Session.
            // Als er nog geen winkelmand bestaat, starten we met een lege lijst.
            var winkelmand = HttpContext.Session.HaalObject<List<WinkelmandItem>>(WinkelmandSleutel) ?? new List<WinkelmandItem>();

            // Stap 4: controleer of dit product al in de winkelmand zit.
            var bestaandItem = winkelmand.FirstOrDefault(i => i.ProductId == product.Id);

            if (bestaandItem == null)
            {
                // Product staat nog niet in de winkelmand.
                // We maken een nieuw WinkelmandItem aan op basis van Product.
                winkelmand.Add(new WinkelmandItem
                {
                    ProductId = product.Id,
                    ProductNaam = product.Naam,
                    Omschrijving = product.Omschrijving,
                    Artikelnummer = product.Artikelnummer,
                    Prijs = product.Prijs,
                    Aantal = Aantal,
                    Icoon = product.Icoon
                });
            }
            else
            {
                // Product bestaat al in de winkelmand.
                // Dan verhogen we alleen het aantal, zodat er geen dubbele regels ontstaan.
                bestaandItem.Aantal += Aantal;
            }

            // Stap 5: sla de gewijzigde winkelmand terug op in Session.
            HttpContext.Session.ZetObject(WinkelmandSleutel, winkelmand);

            TempData["WinkelmandProductNaam"] = product.Naam;
            TempData["WinkelmandProductAantal"] = Aantal;
            TempData["WinkelmandProductIcoon"] = product.Icoon;

            // Blijf op de detailpagina, zodat de klant niet uit de productflow wordt gehaald.
            return RedirectToPage("/Producten/Detail", new { id = product.Id });
        }
    }
}
