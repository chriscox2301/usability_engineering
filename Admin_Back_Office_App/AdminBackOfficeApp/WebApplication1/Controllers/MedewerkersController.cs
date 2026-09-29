using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    /*
        MedewerkersController toont medewerkers.
        - MedewerkerRepository leest de tabel Medewerkers.
        - Index() koppelt aan Views/Medewerkers/Index.cshtml.
    */
    public class MedewerkersController : Controller
    {
        private readonly MedewerkerRepository _medewerkerRepository;

        // Constructor ontvangt MedewerkerRepository via Dependency Injection.
        public MedewerkersController(MedewerkerRepository medewerkerRepository)
        {
            _medewerkerRepository = medewerkerRepository;
        }

        // IActionResult returnt de medewerkersview met List<Medewerker> als @model.
        public IActionResult Index()
        {
            return View(_medewerkerRepository.GetAlleMedewerkers());
        }
    }
}
