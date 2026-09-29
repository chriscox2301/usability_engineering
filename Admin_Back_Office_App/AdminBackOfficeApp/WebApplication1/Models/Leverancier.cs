namespace WebApplication1.Models
{
    /* Model: Leverancier. Een leverancier levert producten aan Matrix Inc. */
    public class Leverancier
    {
        // Primary key uit Leveranciers.Id.
        public int Id { get; set; }
        // Bedrijfsnaam van de leverancier.
        public string Naam { get; set; } = string.Empty;
        // Naam van de contactpersoon bij de leverancier.
        public string Contactpersoon { get; set; } = string.Empty;
        // E-mailadres voor contact of orders.
        public string Email { get; set; } = string.Empty;
        // Telefoonnummer voor contact.
        public string Telefoon { get; set; } = string.Empty;
        // Status van de leverancier, bijvoorbeeld Actief.
        public string Status { get; set; } = "Actief";
    }
}
