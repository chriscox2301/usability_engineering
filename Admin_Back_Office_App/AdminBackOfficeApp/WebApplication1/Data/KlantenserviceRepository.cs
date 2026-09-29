using WebApplication1.Models;

namespace WebApplication1.Data
{
    /*
        Demo-repository voor klantenservice.
        Later kan deze vervangen worden door database-opslag.
    */
    public class KlantenserviceRepository
    {
        private readonly List<Klacht> _klachten = new()
        {
            new Klacht
            {
                Id = 1,
                Klachtnummer = "KL-2025-001",
                KlantNaam = "Sophie de Vries",
                Email = "sophie.devries@example.com",
                Ordernummer = "ORD-2025-1002",
                Onderwerp = "Order onderweg maar nog niet ontvangen",
                Omschrijving = "Klant ziet in de orderhistorie dat de bestelling onderweg is, maar de bezorging is nog niet aangekomen.",
                Status = "Open",
                Prioriteit = "Hoog",
                Datum = new DateTime(2025, 6, 20, 14, 35, 0)
            },
            new Klacht
            {
                Id = 2,
                Klachtnummer = "KL-2025-002",
                KlantNaam = "Daan Jansen",
                Email = "daan.jansen@example.com",
                Ordernummer = "ORD-2025-1001",
                Onderwerp = "Verzoek om afleveropmerking te controleren",
                Omschrijving = "Klant vraagt of de orderopmerking 'bezorg het maar bij de buren' goed is meegenomen.",
                Status = "In behandeling",
                Prioriteit = "Normaal",
                Datum = new DateTime(2025, 6, 19, 9, 10, 0)
            },
            new Klacht
            {
                Id = 3,
                Klachtnummer = "KL-2025-003",
                KlantNaam = "Lotte Bakker",
                Email = "lotte.bakker@example.com",
                Ordernummer = "ORD-2025-0998",
                Onderwerp = "Vraag over retouraanmelding",
                Omschrijving = "Vraag is beantwoord en de klant heeft bevestiging ontvangen.",
                Status = "Afgehandeld",
                Prioriteit = "Laag",
                Datum = new DateTime(2025, 6, 17, 16, 5, 0)
            }
        };

        public List<Klacht> GetAlleKlachten()
        {
            return _klachten.OrderByDescending(k => k.Datum).ToList();
        }

        public int GetAantalOpenstaandeKlachten()
        {
            return _klachten.Count(k => k.IsOpenstaand());
        }
    }
}
