using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    /*
        VoorraadController behandelt voorraadbeheer.
        - ProductRepository leest producten en past voorraad aan.
        - Index() koppelt aan Views/Voorraad/Index.cshtml.
        - Aanpassen(id) koppelt aan Views/Voorraad/Aanpassen.cshtml.
    */
    public class VoorraadController : Controller
    {
        private readonly ProductRepository _productRepository;

        // Constructor ontvangt de repository via Dependency Injection.
        public VoorraadController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET /Voorraad: toont producten gesorteerd van laagste naar hoogste voorraad.
        public IActionResult Index()
        {
            return View(_productRepository.GetAlleProducten()
                .Where(p => p.Status.Equals("Actief", StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Voorraad)
                .ToList());
        }

        // GET /Voorraad/Aanpassen/5: toont één product met een formulier voor nieuwe voorraad.
        [HttpGet]
        public IActionResult Aanpassen(int id)
        {
            var product = _productRepository.GetProductById(id);
            if (product == null)
            {
                TempData["Melding"] = "Product niet gevonden.";
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // POST /Voorraad/Aanpassen: model binding vult id en voorraad vanuit het formulier.
        [HttpPost]
        public IActionResult Aanpassen(int id, int voorraad)
        {
            // Technische validatie: negatieve voorraad mag niet naar SQL Server worden opgeslagen.
            if (voorraad < 0)
            {
                TempData["Melding"] = "Voorraad mag niet negatief zijn.";
                return RedirectToAction("Aanpassen", new { id });
            }

            _productRepository.PasVoorraadAan(id, voorraad);
            TempData["Melding"] = "Voorraad is bijgewerkt.";
            return RedirectToAction("Index");
        }
    }
}
