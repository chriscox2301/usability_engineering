using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Abstractions;
using System.Reflection.Metadata.Ecma335;
using WebApplication1.Interface;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /*
        ProductenController bevat alle acties voor productbeheer.

        Technische relaties:
        - ProductenController gebruikt ProductRepository voor databaseacties.
        - ProductRepository gebruikt DatabaseConnection en SQL Server.
        - Product.cs is het model waarmee productdata door de applicatie beweegt.
        - Views/Producten/*.cshtml zijn de Razor Views die de data tonen of formulieren maken.
    */
    public class ProductenController : Controller
    {
        private readonly IProductRepository _productRepository;

        private readonly ILogger<ProductenController> logger;
        /*
            Constructor:
            Deze constructor ontvangt ProductRepository via Dependency Injection.
            De parameter productRepository wordt opgeslagen in _productRepository.
            Daardoor kan elke action in deze controller dezelfde repository gebruiken.
        */
        public ProductenController(IProductRepository productRepository)
        public ProductenController(IProductRepository productRepository, ILogger<ProductenController> logger)
        {
            _productRepository = productRepository;
            this.logger = logger;
        }

        /*
            GET /Producten
            IActionResult zorgt ervoor dat de action een HTTP-response kan teruggeven.
            In dit geval geeft de action een View terug met een List<Product> als model.

            Parameters zoeken/categorie/status/voorraad komen uit de querystring.
            Voorbeeld: /Producten?zoeken=lager&status=Actief
        */
        public IActionResult Index(string? zoeken, string? categorie, string? status, string? voorraad)
        {
            // Standaard tonen we Actief, zodat Producten in de back-office overeenkomt met de klant-bestelapp.
            status ??= "Actief";

            // GetAlleProducten() haalt eerst alle producten uit SQL Server op.
            // AsEnumerable() maakt het mogelijk om daarna met LINQ in C# te filteren.
            var producten = _productRepository.GetAlleProducten().AsEnumerable();

            if (producten == _productRepository)
            {
                logger.LogInformation("Producten opgehaald uit repository");

            }
            else
            {
                logger.LogWarning("Producten ophalen uit repository gefaald");
            } 


            // Controleert of het zoekveld gevuld is. IsNullOrWhiteSpace voorkomt zoeken op lege tekst.
            if (!string.IsNullOrWhiteSpace(zoeken))
            {
                // Waar één van deze velden de zoektekst bevat, blijft het product in de lijst.
                producten = producten.Where(p =>
                    p.Naam.Contains(zoeken, StringComparison.OrdinalIgnoreCase) ||
                    p.Artikelnummer.Contains(zoeken, StringComparison.OrdinalIgnoreCase) ||
                    p.Merk.Contains(zoeken, StringComparison.OrdinalIgnoreCase));
            }

            // Filtert op categorie wanneer de gebruiker niet voor "Alle" kiest.
            if (!string.IsNullOrWhiteSpace(categorie) && categorie != "Alle")
            {
                producten = producten.Where(p => p.Categorie.Equals(categorie, StringComparison.OrdinalIgnoreCase));
            }

            // Filtert op status, bijvoorbeeld Actief, Concept of Inactief.
            if (!string.IsNullOrWhiteSpace(status) && status != "Alle")
            {
                producten = producten.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            // Filtert op voorraadstatus. Hiervoor wordt de helpermethode in Product.cs gebruikt.
            if (voorraad == "Lage voorraad")
            {
                producten = producten.Where(p => p.HeeftLageVoorraad());
            }
            else if (voorraad == "Ruim op voorraad")
            {
                producten = producten.Where(p => !p.HeeftLageVoorraad());
            }

            // ViewBag geeft losse extra data door aan de View.
            // Hier gebruiken we het om ingevulde filterwaarden zichtbaar te houden in Views/Producten/Index.cshtml.
            ViewBag.Zoeken = zoeken;
            ViewBag.Categorie = categorie;
            ViewBag.Status = status;
            ViewBag.Voorraad = voorraad;
            ViewBag.Categorieen = _productRepository.GetCategorieNamen();

            // ToList() voert de LINQ-filters echt uit en geeft de gefilterde producten door aan de View.
            return View(producten.ToList());
        }

        /*
            GET /Producten/Details/5
            id komt uit de route. ASP.NET Core vult deze parameter automatisch met het getal uit de URL.
        */
        public IActionResult Details(int id)
        {
            // Haalt één product op uit SQL Server.
            var product = _productRepository.GetProductById(id);

            // Als de repository null teruggeeft, bestaat het product niet.
            if (product == null)
            {
                // TempData blijft één redirect beschikbaar en wordt in de layout/view als melding getoond.
                TempData["Melding"] = "Product niet gevonden.";
                return RedirectToAction("Index");
            }

            // Koppelt aan Views/Producten/Details.cshtml met Product als @model.
            return View(product);
        }

        /*
            GET /Producten/Toevoegen
            Toont een leeg formulier.
            [HttpGet] maakt expliciet dat deze action bedoeld is voor het openen van de pagina.
        */
        [HttpGet]
        public IActionResult Toevoegen()
        {
            // Vult ViewBag.Statussen, ViewBag.Categorieen, ViewBag.Leveranciers en ViewBag.Iconen voor dropdowns.
            // Een nieuw product mag alleen als Concept of Actief beginnen.
            VulProductFormulierKeuzes(Product.StartStatussen);

            // Nieuw Product-object met standaardwaarden. De View gebruikt dit als formuliermodel.
            return View(new Product { Status = "Concept", Icoon = "package", LaatstGewijzigd = "Nog niet opgeslagen" });
        }

        /*
            POST /Producten/Toevoegen
            Verwerkt de form submit van Views/Producten/Toevoegen.cshtml.
            ASP.NET Core model binding vult de Product-parameter met waarden uit de form inputs.
        */
        [HttpPost]
        public IActionResult Toevoegen(Product product)
        {
            // Artikelnummer is UNIQUE in SQL Server. We controleren dat vooraf, anders crasht de INSERT.
            // De key "Artikelnummer" koppelt de fout aan dat veld, zodat de View het juiste vlak rood maakt.
            if (!string.IsNullOrWhiteSpace(product.Artikelnummer) &&
                _productRepository.GetAlleProducten().Any(p => p.Artikelnummer.Equals(product.Artikelnummer.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(Product.Artikelnummer), "Dit artikelnummer bestaat al.");
            }

            // De keuzelijst toont alleen Concept en Actief, maar een formulier kan ook handmatig aangepast worden.
            if (!Product.StartStatussen.Contains(product.Status))
            {
                ModelState.AddModelError(nameof(Product.Status), "Een nieuw product kan alleen de status Concept of Actief krijgen.");
            }

            // ModelState.IsValid is false als een regel uit Product.cs ([Required], [Range], ...) niet klopt.
            if (!ModelState.IsValid)
            {
                // Dropdownlijsten opnieuw vullen, anders zijn ze leeg wanneer de View opnieuw getoond wordt.
                VulProductFormulierKeuzes(Product.StartStatussen);
                return View(product);
            }

            // Stuurt het Product-object naar de repository. De repository voert de INSERT stored procedure uit.
            _productRepository.VoegProductToe(product);

            // Na opslaan redirecten we naar Index zodat refresh niet opnieuw hetzelfde product toevoegt.
            TempData["Melding"] = "Product is toegevoegd aan SQL Server en staat nu in het productoverzicht.";
            return RedirectToAction("Index");
        }

        /*
            GET /Producten/Wijzigen/5
            Haalt bestaand product op en toont het wijzigformulier.
        */
        [HttpGet]
        public IActionResult Wijzigen(int id)
        {
            var product = _productRepository.GetProductById(id);
            if (product == null)
            {
                TempData["Melding"] = "Product niet gevonden.";
                return RedirectToAction("Index");
            }

            // Keuzelijsten vullen voordat Views/Producten/Wijzigen.cshtml gerenderd wordt.
            // De statuslijst bevat alleen de statussen waar dit product naartoe mag.
            VulProductFormulierKeuzes(product.ToegestaneStatussen());
            return View(product);
        }

        /*
            POST /Producten/Wijzigen
            Slaat gewijzigde productgegevens op.
            De hidden input asp-for="Id" in de View zorgt dat het product-id meegestuurd wordt.
        */
        [HttpPost]
        public IActionResult Wijzigen(Product product)
        {
            // We halen de huidige versie uit de database op, want alleen die vertelt wat de status nú is.
            var bestaandProduct = _productRepository.GetProductById(product.Id);
            if (bestaandProduct == null)
            {
                TempData["Melding"] = "Product niet gevonden.";
                return RedirectToAction("Index");
            }

            // Wijzigen.cshtml toont alleen algemene fouten (ModelOnly), daarom gebruiken we hier string.Empty als key.
            var heeftFout = false;
            if (string.IsNullOrWhiteSpace(product.Naam) || string.IsNullOrWhiteSpace(product.Artikelnummer))
            {
                ModelState.AddModelError(string.Empty, "Naam en artikelnummer zijn verplicht.");
                heeftFout = true;
            }

            // Controleert de statusregels uit Product.cs, bijvoorbeeld: Actief mag niet terug naar Concept.
            if (!bestaandProduct.MagNaarStatus(product.Status))
            {
                ModelState.AddModelError(string.Empty, $"Een product met status {bestaandProduct.Status} kan niet naar {product.Status}.");
                heeftFout = true;
            }

            if (heeftFout)
            {
                VulProductFormulierKeuzes(bestaandProduct.ToegestaneStatussen());
                return View(product);
            }

            // WijzigProduct voert sp_Producten_Update uit.
            _productRepository.WijzigProduct(product);
            TempData["Melding"] = "Productgegevens zijn bijgewerkt in SQL Server.";
            return RedirectToAction("Index");
        }

        /*
            GET /Producten/Verwijderen/5
            Toont eerst een bevestigingspagina.
        */
        [HttpGet]
        public IActionResult Verwijderen(int id)
        {
            var product = _productRepository.GetProductById(id);
            if (product == null)
            {
                TempData["Melding"] = "Product niet gevonden.";
                return RedirectToAction("Index");
            }

            // Een inactief product kan niet verder verwijderd worden. Het kan alleen weer op Actief gezet worden.
            if (product.Status == "Inactief")
            {
                TempData["Melding"] = "Dit product is al inactief. Via Wijzigen kun je het weer op Actief zetten.";
                return RedirectToAction("Index");
            }

            // Koppelt aan Views/Producten/Verwijderen.cshtml. De View past de tekst aan op de status.
            return View(product);
        }

        /*
            POST /Producten/VerwijderenBevestigd
            - Concept: wordt definitief verwijderd. Een Concept is nooit besteld geweest.
            - Actief: wordt op Inactief gezet in plaats van fysiek verwijderd.
              Daardoor blijven historische bestellingen veilig gekoppeld aan bestaande product-id's.
            - Inactief: niets doen, dat is al de eindstatus van Verwijderen.
        */
        [HttpPost]
        public IActionResult VerwijderenBevestigd(int id)
        {
            var product = _productRepository.GetProductById(id);
            if (product == null)
            {
                TempData["Melding"] = "Product niet gevonden.";
                return RedirectToAction("Index");
            }

            if (product.KanDefinitiefVerwijderdWorden())
            {
                TempData["Melding"] = _productRepository.VerwijderConceptProduct(id)
                    ? "Conceptproduct is definitief verwijderd."
                    : "Dit conceptproduct kan niet verwijderd worden, omdat er bestelregels aan gekoppeld zijn.";
            }
            else if (product.Status == "Actief")
            {
                _productRepository.DeactiveerProduct(id);
                TempData["Melding"] = "Product is in SQL Server op Inactief gezet. Het product is niet fysiek verwijderd.";
            }
            else
            {
                TempData["Melding"] = "Dit product is al inactief.";
            }

            return RedirectToAction("Index");
        }

        /*
            Private helpermethode:
            Deze methode hoort alleen bij deze controller en kan niet via een URL worden aangeroepen.
            De methode voorkomt dubbele code in Toevoegen() en Wijzigen().
        */
        private void VulProductFormulierKeuzes(List<string> statussen)
        {
            // Deze lijsten worden in de Views gebruikt door foreach-loops om <option>-tags te maken.
            // statussen komt uit Product.StartStatussen (Toevoegen) of product.ToegestaneStatussen() (Wijzigen).
            ViewBag.Statussen = statussen;
            ViewBag.Categorieen = _productRepository.GetCategorieNamen();
            ViewBag.Leveranciers = _productRepository.GetLeverancierNamen();
            ViewBag.Iconen = new List<string> { "package", "circle-dot", "cog", "bolt", "wrench", "rotate-cw", "gauge" };
        }
    }
}
