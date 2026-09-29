using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    /*
        CategorieenController behandelt de categoriepagina's.
        - CategorieRepository leest categorieën uit SQL Server.
        - Index() toont de totale lijst in Views/Categorieen/Index.cshtml.
        - Beheren(id) toont één categorie in Views/Categorieen/Beheren.cshtml.
    */
    public class CategorieenController : Controller
    {
        private readonly CategorieRepository _categorieRepository;

        // Constructor: de repository wordt door Dependency Injection doorgegeven vanuit Program.cs.
        public CategorieenController(CategorieRepository categorieRepository)
        {
            _categorieRepository = categorieRepository;
        }

        // IActionResult geeft een View terug met List<Categorie> als model.
        public IActionResult Index()
        {
            return View(_categorieRepository.GetAlleCategorieen());
        }

        // id komt uit /Categorieen/Beheren/5 en wordt gebruikt om één categorie te zoeken.
        public IActionResult Beheren(int id)
        {
            var categorie = _categorieRepository.GetCategorieById(id);
            if (categorie == null)
            {
                TempData["Melding"] = "Categorie niet gevonden.";
                return RedirectToAction("Index");
            }

            return View(categorie);
        }
    }
}
