namespace WebApplication1.Models
{
    /* Model: Categorie. Een categorie groepeert producten, bijvoorbeeld Lagers of Hydrauliek. */
    public class Categorie
    {
        // Primary key uit Categorieen.Id.
        public int Id { get; set; }
        // Naam die in dropdowns en categorieoverzichten getoond wordt.
        public string Naam { get; set; } = string.Empty;
        // Uitlegtekst over de categorie.
        public string Omschrijving { get; set; } = string.Empty;
        // Aantal gekoppelde producten. Wordt berekend in de SQL-query met COUNT(p.Id).
        public int AantalProducten { get; set; }
        // Lucide-icoonnaam voor visuele herkenning in de view.
        public string Icoon { get; set; } = "folder";
    }
}
