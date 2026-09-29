using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    /*
        InstellingenController toont de instellingenpagina.
        Er wordt geen repository gebruikt omdat deze pagina nu statische instellingen toont.
        Index() koppelt automatisch aan Views/Instellingen/Index.cshtml.
    */
    public class InstellingenController : Controller
    {
        // IActionResult geeft een ViewResult terug. Er is geen model nodig voor deze pagina.
        public IActionResult Index()
        {
            return View();
        }
    }
}
