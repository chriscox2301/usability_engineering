using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    /*
        Product-model in lijn met de MVC back-office database.
        Dit model wordt gevuld vanuit Producten + Categorieen + Leveranciers in MatrixIncDb.
    */
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Naam { get; set; } = string.Empty;

        public string Omschrijving { get; set; } = string.Empty;
        public decimal Prijs { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public string Merk { get; set; } = string.Empty;
        public int Voorraad { get; set; }
        public string Status { get; set; } = "Actief";
        public string Icoon { get; set; } = "package";
        public string Artikelnummer { get; set; } = string.Empty;
        public string Leverancier { get; set; } = string.Empty;
        public string LaatstGewijzigd { get; set; } = string.Empty;

        // Deze velden gebruikt de klantapp op detailpagina's. Ze bestaan niet los in de back-office tabel,
        // daarom worden ze in ProductRepository afgeleid uit categorie/omschrijving/artikelnummer.
        public string Materiaal { get; set; } = string.Empty;
        public string Afmeting { get; set; } = string.Empty;
        public string Levertijd { get; set; } = "2 werkdagen";


        public bool HeeftLageVoorraad() => Voorraad <= 5 && Status.Equals("Actief", StringComparison.OrdinalIgnoreCase);
        public bool IsConcept() => Status.Equals("Concept", StringComparison.OrdinalIgnoreCase);
    }
}
