using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace KE03_INTDEV_SE_1_Base.Pages.Bestellingen
{
    public class PlaatsenModel : PageModel
    {
        private readonly IBestellingRepository _bestellingRepository;
        private const string WinkelmandSleutel = "Winkelmand";

        public List<WinkelmandItem> Items { get; set; } = new();
        public decimal Totaal => Items.Sum(i => i.Subtotaal);
        public bool Besteld { get; set; }

		public bool GastFormulierOpen { get; set; }
		public string LaatsteOrdernummer { get; set; } = string.Empty;
        public string? Foutmelding { get; set; }

        [BindProperty]
		[Required(ErrorMessage = "Vul je naam in.")]
		[StringLength(100, MinimumLength = 2 ,ErrorMessage = "Naam moet tussen 2 en 100 tekens zijn.")]
		public string Naam { get; set; } = string.Empty;

        [BindProperty]
		[Required(ErrorMessage = "Vul je e-mailadres in.")]
		[EmailAddress(ErrorMessage = "Vul een geldig e-mailadres in, bijvoorbeeld naam@example.com.")]
		public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Vul je adres in.")]
        [StringLength(150, ErrorMessage = "Je adres mag maximaal 150 tekens bevatten.")]
		[RegularExpression(@"^.*\d.*$", ErrorMessage = "Vul ook een huisnummer in, bijvoorbeeld Hoofdstraat 12.")]
		public string Adres { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Vul je postcode in.")]
		[RegularExpression(
	        @"^[1-9][0-9]{3}\s?[A-Za-z]{2}$",
	        ErrorMessage = "Vul een geldige Nederlandse postcode in, bijvoorbeeld 1234 AB.")]
		public string Postcode { get; set; } = string.Empty;

        [BindProperty]
		[Required(ErrorMessage = "Vul je woonplaats in.")]
		[StringLength(100, MinimumLength = 2, ErrorMessage = "Woonplaats moet tussen 2 en 100 tekens zijn.")]
		[RegularExpression(@"^\D+$", ErrorMessage = "Woonplaats mag geen cijfers bevatten.")]
		public string Plaats { get; set; } = string.Empty;

        [BindProperty]
        public string? OrderOpmerkingen { get; set; }

        [BindProperty]
        public string Betaalmethode { get; set; } = "iDEAL";

        public PlaatsenModel(IBestellingRepository bestellingRepository)
        {
            _bestellingRepository = bestellingRepository;
        }

        public void OnGet()
        {
            Items = HttpContext.Session.HaalObject<List<WinkelmandItem>>(WinkelmandSleutel) ?? new List<WinkelmandItem>();
        }

        public IActionResult OnPost()
        {
			GastFormulierOpen = true;

			Items = HttpContext.Session.HaalObject<List<WinkelmandItem>>(WinkelmandSleutel) ?? new List<WinkelmandItem>();

            if (!Items.Any())
            {
                return Page();
            }

			if (!ModelState.IsValid)
			{
				return Page();
			}

			var bestelling = new GeplaatsteBestelling
            {
                Ordernummer = $"ORD-{DateTime.Now:yyyyMMdd-HHmmss}",
                Besteldatum = DateTime.Now,
                Status = "Nieuw",
                Totaalbedrag = Totaal,
                KlantNaam = Naam,
                Email = Email,
                VerzendAdres = $"{Adres}, {Postcode} {Plaats}",
                OrderOpmerkingen = OrderOpmerkingen,
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
