using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    /*
        LeveranciersController toont leveranciers.
        - LeverancierRepository haalt data uit de tabel Leveranciers.
        - Index() koppelt aan Views/Leveranciers/Index.cshtml.
    */
    public class LeveranciersController : Controller
    {
        private readonly LeverancierRepository _leverancierRepository;

        // Constructor ontvangt LeverancierRepository via Dependency Injection.
        public LeveranciersController(LeverancierRepository leverancierRepository)
        {
            _leverancierRepository = leverancierRepository;
        }

        // Geeft alle leveranciers door aan de View als List<Leverancier>.
        public IActionResult Index()
        {
            return View(_leverancierRepository.GetAlleLeveranciers());
        }
    }
}
