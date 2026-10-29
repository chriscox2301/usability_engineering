using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using Microsoft.Extensions.Logging;
using WebApplication1.Interface;
namespace WebApplication1.Controllers;

/*
    BestellingenController toont en verwerkt orders.

    Relatie met klant-bestelapplicatie:
    - Klantapp schrijft bestellingen in dezelfde SQL Server tabellen.
    - Deze controller laat die orders direct in de back-office zien.
    - Annulering/klacht/status blijven in dezelfde Bestellingen.Status kolom staan.
*/
public class BestellingenController : Controller
{
    private readonly IBestellingRepository _bestellingRepository;
    private readonly ILogger<BestellingenController> logger;

    public BestellingenController(
        IBestellingRepository bestellingRepository,
        ILogger<BestellingenController> logger)
    {
        _bestellingRepository = bestellingRepository;
        this.logger = logger;
    }

    public IActionResult Index(string? klant, string? status, string? prioriteit)
    {
        var bestellingen = _bestellingRepository.GetAlleBestellingen().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(klant))
        {
            bestellingen = bestellingen.Where(b =>
                b.KlantNaam.Contains(klant, StringComparison.OrdinalIgnoreCase) ||
                b.Ordernummer.Contains(klant, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "Alle")
        {
            bestellingen = bestellingen.Where(b => b.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(prioriteit) && prioriteit != "Alle")
        {
            bestellingen = bestellingen.Where(b => b.Prioriteit.Equals(prioriteit, StringComparison.OrdinalIgnoreCase));
        }

        ViewBag.Klant = klant;
        ViewBag.Status = status;
        ViewBag.Prioriteit = prioriteit;

        return View(bestellingen.ToList());
    }

    public IActionResult Details(int id)
    {
        var bestelling = _bestellingRepository.GetBestellingById(id);
        if (bestelling == null)
        {
            TempData["Melding"] = "Bestelling niet gevonden.";
            return RedirectToAction("Index");
        }

        ViewBag.Bezorgers = _bestellingRepository.GetBezorgers();
        return View(bestelling);
    }

    [HttpPost]
    public IActionResult RegelInpakken(int id, int regelId, bool isIngepakt)
    {
        // Oude losse checkbox-actie blijft bestaan voor eventuele bestaande links/forms.
        // De orderdetails pagina gebruikt nu RegelsInpakken, zodat de medewerker eerst kan bevestigen.
        _bestellingRepository.UpdateBestelregelIngepakt(regelId, isIngepakt);
        TempData["Melding"] = isIngepakt
            ? "Orderregel is aangevinkt als ingepakt."
            : "Orderregel is teruggezet naar niet ingepakt.";

        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    public IActionResult RegelsInpakken(int id, List<int> ingepakteRegelIds)
    {
        _bestellingRepository.UpdateInpaklijst(id, ingepakteRegelIds ?? new List<int>());
        TempData["Melding"] = "Inpaklijst is bevestigd en opgeslagen.";

        return RedirectToAction("Details", new { id });
    }


		[HttpPost]
		public IActionResult StatusWijzigen(int id, string status)
		{
			bool gelukt = _bestellingRepository.UpdateStatus(id, status);


			if (gelukt)
			{
				logger.LogInformation(
					"Status van bestelling {Id} gewijzigd naar {Status}.",
					id, status);

				TempData["Melding"] = $"Status is aangepast naar '{status}'.";
			}
			else
			{
				logger.LogWarning(
					"Status van bestelling {Id} kon niet worden aangepast.",
					id);

				TempData["Melding"] =
					"Status niet aangepast: alle orderpicker-checkboxen moeten eerst aangevinkt zijn.";
			}

			return RedirectToAction("Details", new { id });
		}


		[HttpPost]
    public IActionResult BezorgerKoppelen(int id, int? bezorgerId)
    {
        _bestellingRepository.UpdateBezorger(id, bezorgerId);
        TempData["Melding"] = bezorgerId.HasValue
            ? "Bezorger is gekoppeld. De order komt nu in de bezorglijst van deze medewerker."
            : "Bezorger is losgekoppeld.";

        return RedirectToAction("Details", new { id });
    }
}
