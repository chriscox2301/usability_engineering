using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /*
        HomeController bevat de algemene foutpagina.
        Program.cs verwijst naar /Home/Error wanneer er in productie een exception optreedt.
    */
    public class HomeController : Controller
    {
        // IActionResult koppelt aan Views/Shared/Error.cshtml en geeft ErrorViewModel mee.
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                // Activity.Current?.Id is beschikbaar wanneer tracing actief is.
                // HttpContext.TraceIdentifier is de fallback request-id van ASP.NET Core.
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
