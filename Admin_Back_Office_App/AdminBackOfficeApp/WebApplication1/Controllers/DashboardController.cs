using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Interface;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /*
        DashboardController is de controller voor de startpagina.

        Technische relaties:
        - Deze controller ontvangt ProductRepository en BestellingRepository via de constructor.
        - ProductRepository haalt Product-objecten op uit SQL Server.
        - BestellingRepository haalt Bestelling-objecten op uit SQL Server.
        - AdminDashboardViewModel bundelt alle data die de dashboard-view nodig heeft.
        - return View(model) koppelt deze action aan Views/Dashboard/Index.cshtml.
    */
    public class DashboardController : Controller
    {
        // private readonly betekent: deze variabele wordt alleen in de constructor gezet en daarna niet vervangen.
        private readonly IProductRepository _productRepository;
        private readonly BestellingRepository _bestellingRepository;
        private readonly KlantenserviceRepository _klantenserviceRepository;

        /*
            Constructor:
            ASP.NET Core ziet in Program.cs dat ProductRepository en BestellingRepository geregistreerd zijn.
            Daarom geeft Dependency Injection deze objecten automatisch mee aan deze constructor.
        */
        public DashboardController(IProductRepository productRepository, BestellingRepository bestellingRepository, KlantenserviceRepository klantenserviceRepository)
        {
            // De parameters worden opgeslagen in private velden zodat alle actions ze kunnen gebruiken.
            _productRepository = productRepository;
            _bestellingRepository = bestellingRepository;
            _klantenserviceRepository = klantenserviceRepository;
        }

        /*
            IActionResult:
            Een action method geeft meestal IActionResult terug.
            Dat is een algemeen resultaat-type waarmee ASP.NET Core meerdere soorten responses kan teruggeven:
            - View(model): toon een Razor View met data.
            - RedirectToAction(...): stuur door naar een andere action.
            - NotFound(): geef een 404 terug.

            Deze action hoort bij GET /Dashboard en bij de standaard startpagina /.
        */
        public IActionResult Index()
        {
            // Haalt alle producten op. Intern opent ProductRepository een SqlConnection en voert SQL/stored procedures uit.
            var producten = _productRepository.GetAlleProducten();

            // Haalt alle bestellingen op. De repository geeft een List<Bestelling> terug.
            var bestellingen = _bestellingRepository.GetAlleBestellingen();
            var klachten = _klantenserviceRepository.GetAlleKlachten();

            // Een ViewModel is een model dat specifiek is samengesteld voor één View.
            // Hier voorkomen we losse ViewBag-waarden en houden we de dashboarddata overzichtelijk bij elkaar.
            var model = new AdminDashboardViewModel
            {
                // Telt producten waarbij de modelmethode HeeftLageVoorraad() true teruggeeft.
                AantalLageVoorraadProducten = producten.Count(p => p.HeeftLageVoorraad()),

                // Telt orders met status Nieuw. De Status-property komt uit de databasekolom Bestellingen.Status.
                AantalNieuweBestellingen = bestellingen.Count(b => b.Status == "Nieuw"),

                // Telt producten die nog in concept staan. De IsConcept()-methode staat in Product.cs.
                AantalOpenstaandeKlachten = klachten.Count(k => k.IsOpenstaand()),

                // Telt open bestellingen met prioriteit Hoog.
                AantalHogePrioriteitBestellingen = bestellingen.Count(b => b.Prioriteit == "Hoog" && b.Status != "Bezorgd" && b.Status != "Geannuleerd"),

                // Deze lijsten worden onder de KPI-kaarten getoond in de dashboard-view.
                ProductenMetLageVoorraad = producten.Where(p => p.HeeftLageVoorraad()).OrderBy(p => p.Voorraad).ToList(),
                OpenstaandeKlachten = klachten.Where(k => k.IsOpenstaand()).ToList(),
                TeVerwerkenBestellingen = bestellingen.Where(b => b.Status == "Nieuw").ToList(),
                HogePrioriteitBestellingen = bestellingen.Where(b => b.Prioriteit == "Hoog" && b.Status != "Bezorgd" && b.Status != "Geannuleerd").ToList()
            };

            // ASP.NET Core zoekt automatisch naar Views/Dashboard/Index.cshtml.
            // Het model is daar beschikbaar via @model AdminDashboardViewModel.
            return View(model);
        }
    }
}
