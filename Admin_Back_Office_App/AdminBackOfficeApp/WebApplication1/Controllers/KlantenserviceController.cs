using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public class KlantenserviceController : Controller
    {
        private readonly KlantenserviceRepository _klantenserviceRepository;

        public KlantenserviceController(KlantenserviceRepository klantenserviceRepository)
        {
            _klantenserviceRepository = klantenserviceRepository;
        }

        public IActionResult Index()
        {
            return View(_klantenserviceRepository.GetAlleKlachten());
        }
    }
}
