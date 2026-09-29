using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages.Bestellingen
{
    public class BetalenModel : PageModel
    {
        private readonly IBestellingRepository _bestellingRepository;
        private const string WinkelmandSleutel = "Winkelmand";

        public List<WinkelmandItem> Items { get; set; } = new();
        public decimal Totaal => Items.Sum(i => i.Subtotaal);
        public bool Besteld { get; set; }
        public string LaatsteOrdernummer { get; set; } = string.Empty;
        public string? Foutmelding { get; set; }

        [BindProperty]
        public string Naam { get; set; } = "Jan";

        [BindProperty]
        public string Email { get; set; } = "Jan@zuyd.com";

        [BindProperty]
        public string Straat { get; set; } = "Straat";

        [BindProperty]
        public string Huisnummer { get; set; } = "34";

        [BindProperty]
        public string Postcode { get; set; } = "5611PO";

        [BindProperty]
        public string Plaats { get; set; } = "Eindhoven";

        [BindProperty]
        public string Betaalmethode { get; set; } = "iDEAL";

        [BindProperty]
        public string OrderOpmerkingen { get; set; } = string.Empty;

        public BetalenModel(IBestellingRepository bestellingRepository)
        {
            _bestellingRepository = bestellingRepository;
        }

        public void OnGet()
        {
            Items = HttpContext.Session.HaalObject<List<WinkelmandItem>>(WinkelmandSleutel) ?? new List<WinkelmandItem>();
        }

        public IActionResult OnPost()
        {
            Items = HttpContext.Session.HaalObject<List<WinkelmandItem>>(WinkelmandSleutel) ?? new List<WinkelmandItem>();

            if (!Items.Any())
            {
                return Page();
            }

            var bestelling = new GeplaatsteBestelling
            {
                Ordernummer = $"ORD-{DateTime.Now:yyyyMMdd-HHmmss}",
                Besteldatum = DateTime.Now,
                Status = "Nieuw",
                Totaalbedrag = Totaal,
                KlantNaam = string.IsNullOrWhiteSpace(Naam) ? "Jan" : Naam,
                Email = string.IsNullOrWhiteSpace(Email) ? "Jan@zuyd.com" : Email,
                VerzendAdres = $"{Straat} {Huisnummer}, {Postcode} {Plaats}".Trim(),
                OrderOpmerkingen = string.IsNullOrWhiteSpace(OrderOpmerkingen)
                    ? $"Betaalmethode: {Betaalmethode}"
                    : $"Betaalmethode: {Betaalmethode}. {OrderOpmerkingen}",
                Regels = Items.Select(i => new Bestelregel
                {
                    ProductId = i.ProductId,
                    ProductNaam = i.ProductNaam,
                    Artikelnummer = string.IsNullOrWhiteSpace(i.Artikelnummer) ? $"P-{i.ProductId}" : i.Artikelnummer,
                    Prijs = i.Prijs,
                    Aantal = i.Aantal
                }).ToList()
            };

            try
            {
                LaatsteOrdernummer = _bestellingRepository.PlaatsBestelling(bestelling);
                HttpContext.Session.ZetObject(WinkelmandSleutel, new List<WinkelmandItem>());
                Items = new List<WinkelmandItem>();
                Besteld = true;
            }
            catch (InvalidOperationException ex)
            {
                Foutmelding = ex.Message;
                Besteld = false;
            }

            return Page();
        }
    }
}
