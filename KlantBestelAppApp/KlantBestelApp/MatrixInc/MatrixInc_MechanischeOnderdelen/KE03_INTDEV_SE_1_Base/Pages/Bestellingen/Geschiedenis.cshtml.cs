using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages.Bestellingen
{
    public class GeschiedenisModel : PageModel
    {
        private readonly IBestellingRepository _bestellingRepository;

        public List<GeplaatsteBestelling> Bestellingen { get; set; } = new();

        [TempData]
        public string? ActieMelding { get; set; }

        public GeschiedenisModel(IBestellingRepository bestellingRepository)
        {
            _bestellingRepository = bestellingRepository;
        }

        public void OnGet()
        {
            LaadBestellingen();
        }

        public IActionResult OnPostAnnuleer(string ordernummer)
        {
            _bestellingRepository.VraagAnnuleringAan(ordernummer);
            ActieMelding = $"Annulering aangevraagd voor order {ordernummer}. De status is opgeslagen in MatrixIncDb.";
            return RedirectToPage();
        }

        public IActionResult OnPostKlacht(string ordernummer)
        {
            _bestellingRepository.RegistreerKlachtmelding(ordernummer);
            ActieMelding = $"Klacht gemeld voor order {ordernummer}. De status is opgeslagen in MatrixIncDb.";
            return RedirectToPage();
        }

        private void LaadBestellingen()
        {
            Bestellingen = _bestellingRepository.HaalOrderhistorieOp();
        }
    }
}
