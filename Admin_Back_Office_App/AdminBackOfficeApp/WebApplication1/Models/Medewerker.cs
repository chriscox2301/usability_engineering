namespace WebApplication1.Models
{
    /* Model: Medewerker. Dit model stelt een back-office medewerker voor. */
    public class Medewerker
    {
        // Primary key uit Medewerkers.Id.
        public int Id { get; set; }
        // Naam van de medewerker.
        public string Naam { get; set; } = string.Empty;
        // Rol/functie van de medewerker.
        public string Rol { get; set; } = string.Empty;
        // E-mailadres van de medewerker.
        public string Email { get; set; } = string.Empty;
        // Laatste activiteit als tekst. Repository maakt hier een leesbaar datumformaat van.
        public string LaatstActief { get; set; } = string.Empty;
    }
}
