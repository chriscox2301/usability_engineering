using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    /*
        RapportagesController toont simpele rapportage-informatie op basis van producten.
        - ProductRepository haalt productdata op uit SQL Server.
        - Views/Rapportages/Index.cshtml gebruikt de productlijst voor tellingen en tabellen.
    */
    public class RapportagesController : Controller
    {
        private readonly ProductRepository _productRepository;

        // Constructor ontvangt ProductRepository via Dependency Injection.
        public RapportagesController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Geeft alle producten aan de rapportageview.
        public IActionResult Index()
        {
            return View(_productRepository.GetAlleProducten());
        }
    }
}
