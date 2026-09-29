namespace WebApplication1.Models
{
    /*
        Model voor klantenservice-klachten.
        Voor deze demo gebruiken we hardcoded klachten in KlantenserviceRepository.
        Status "Open" en "In behandeling" tellen als nog niet afgehandeld.
    */
    public class Klacht
    {
        public int Id { get; set; }
        public string Klachtnummer { get; set; } = string.Empty;
        public string KlantNaam { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Ordernummer { get; set; } = string.Empty;
        public string Onderwerp { get; set; } = string.Empty;
        public string Omschrijving { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Prioriteit { get; set; } = string.Empty;
        public DateTime Datum { get; set; }

        public bool IsOpenstaand()
        {
            return Status != "Afgehandeld";
        }
    }
}
