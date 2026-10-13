using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    /*
        Model: Product

        Een model is een C# klasse die data voorstelt binnen de applicatie.
        Dit model stelt één product voor. Repositories vullen dit model met data uit SQL Server.
        Controllers geven Product-objecten door aan Views, bijvoorbeeld Views/Producten/Index.cshtml.
    */
    public class Product
    {
        // Primary key uit de databasekolom Producten.Id.
        public int Id { get; set; }

        // Naam die op schermen en formulieren wordt getoond.
        // [Required] en [StringLength] zijn validatieregels; de controller controleert ze via ModelState.IsValid.
        [Required(ErrorMessage = "Vul een naam in.")]
        [StringLength(100, ErrorMessage = "Naam mag maximaal 100 tekens zijn.")]
        public string Naam { get; set; } = string.Empty;

        // Lange tekst met extra informatie over het product.
        [Required(ErrorMessage = "Vul een omschrijving in.")]
        [StringLength(500, ErrorMessage = "Omschrijving mag maximaal 500 tekens zijn.")]
        public string Omschrijving { get; set; } = string.Empty;

        // Uniek artikelnummer. Dit wordt gebruikt om producten snel te herkennen.
        [Required(ErrorMessage = "Vul een artikelnummer in.")]
        [StringLength(50, ErrorMessage = "Artikelnummer mag maximaal 50 tekens zijn.")]
        public string Artikelnummer { get; set; } = string.Empty;

        // Merknaam van het product.
        [Required(ErrorMessage = "Vul een merk in.")]
        [StringLength(100, ErrorMessage = "Merk mag maximaal 100 tekens zijn.")]
        public string Merk { get; set; } = string.Empty;

        // Categorienaam zoals 'Lagers' of 'Hydrauliek'. In SQL Server is dit gekoppeld via CategorieId.
        [Required(ErrorMessage = "Kies een categorie.")]
        public string Categorie { get; set; } = string.Empty;

        // Verkoopprijs of interne productprijs. Decimal wordt gebruikt voor geldbedragen.
        [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Prijs moet tussen € 0,01 en € 99.999.999,99 liggen.")]
        public decimal Prijs { get; set; }

        // Aantal stuks dat op voorraad is.
        [Range(0, int.MaxValue, ErrorMessage = "Voorraad kan niet negatief zijn.")]
        public int Voorraad { get; set; }

        // Productstatus, bijvoorbeeld Actief, Concept of Inactief.
        public string Status { get; set; } = "Actief";

        // Naam van het Lucide-icoon dat in de interface getoond wordt.
        public string Icoon { get; set; } = "package";

        // Leveranciersnaam. In SQL Server is dit gekoppeld via LeverancierId.
        public string Leverancier { get; set; } = string.Empty;

        // Geformatteerde datum/tijd als tekst voor weergave in de View.
        public string LaatstGewijzigd { get; set; } = string.Empty;

        // Business helper: true als de voorraad laag is en het product actief is.
        public bool HeeftLageVoorraad()
        {
            return Voorraad <= 5 && Status.Equals("Actief", StringComparison.OrdinalIgnoreCase);
        }

        // Business helper: true als de productstatus Concept is.
        public bool IsConcept()
        {
            return Status.Equals("Concept", StringComparison.OrdinalIgnoreCase);
        }

        // Business helper: controleert of belangrijke velden ontbreken of ongeldig zijn.
        public bool HeeftOnvolledigeInformatie()
        {
            return string.IsNullOrWhiteSpace(Naam)
                || string.IsNullOrWhiteSpace(Omschrijving)
                || string.IsNullOrWhiteSpace(Artikelnummer)
                || string.IsNullOrWhiteSpace(Merk)
                || string.IsNullOrWhiteSpace(Categorie)
                || Prijs <= 0;
        }
    }
}
