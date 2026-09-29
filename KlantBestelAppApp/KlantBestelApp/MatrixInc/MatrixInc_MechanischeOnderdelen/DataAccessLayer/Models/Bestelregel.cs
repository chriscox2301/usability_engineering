namespace DataAccessLayer.Models
{
    public class Bestelregel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductNaam { get; set; } = string.Empty;
        public string Artikelnummer { get; set; } = string.Empty;
        public string Omschrijving { get; set; } = string.Empty;
        public string Merk { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public string Icoon { get; set; } = "package";
        public decimal Prijs { get; set; }
        public int Aantal { get; set; }
        public int Voorraad { get; set; }
        public bool IsIngepakt { get; set; }
        public decimal Subtotaal => Prijs * Aantal;
        public decimal RegelTotaal => Prijs * Aantal;
    }
}
