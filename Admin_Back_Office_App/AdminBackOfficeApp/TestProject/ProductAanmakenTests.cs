using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace TestProject
{
    /*
        Unit tests voor het aanmaken van een product.

        Deze tests controleren de validatieregels in Product.cs ([Required], [Range], ...).
        Dat zijn dezelfde regels die ProductenController.Toevoegen via ModelState.IsValid gebruikt:
        is het model geldig, dan wordt het product aangemaakt; anders krijgt de gebruiker een validatiefout.

        Elke test volgt het Arrange-Act-Assert patroon:
        - Arrange: de testdata klaarzetten.
        - Act:     de handeling uitvoeren die we testen.
        - Assert:  controleren of de uitkomst klopt.
    */
    public class ProductAanmakenTests
    {
        // TC01 - Product aanmaken - Geldige productnaam en prijs - Product wordt aangemaakt
        [Fact]
        public void TC01_ProductAanmaken_GeldigeNaamEnPrijs_ProductWordtAangemaakt()
        {
            // Arrange: een product met een geldige naam en een geldige prijs.
            var product = new Product
            {
                Naam = "Kogellager 6204",
                Omschrijving = "Diepgroefkogellager voor algemene toepassingen.",
                Artikelnummer = "KL-6204",
                Merk = "SKF",
                Categorie = "Lagers",
                Prijs = 12.50m,
                Voorraad = 10,
                Status = "Concept"
            };
            var fouten = new List<ValidationResult>();
            var context = new ValidationContext(product);

            // Act: het product valideren zoals ASP.NET Core dat doet bij het opslaan.
            var isGeldig = Validator.TryValidateObject(product, context, fouten, validateAllProperties: true);

            // Assert: het product is geldig, dus het wordt aangemaakt.
            Assert.True(isGeldig);
            Assert.Empty(fouten);
        }

        // TC02 - Product aanmaken - Negatieve prijs - Validatiefout
        [Fact]
        public void TC02_ProductAanmaken_NegatievePrijs_Validatiefout()
        {
            // Arrange: een product dat verder geldig is, maar een negatieve prijs heeft.
            var product = new Product
            {
                Naam = "Kogellager 6204",
                Omschrijving = "Diepgroefkogellager voor algemene toepassingen.",
                Artikelnummer = "KL-6204",
                Merk = "SKF",
                Categorie = "Lagers",
                Prijs = -5.00m,
                Voorraad = 10,
                Status = "Concept"
            };
            var fouten = new List<ValidationResult>();
            var context = new ValidationContext(product);

            // Act: het product valideren zoals ASP.NET Core dat doet bij het opslaan.
            var isGeldig = Validator.TryValidateObject(product, context, fouten, validateAllProperties: true);

            // Assert: het product is ongeldig en er is precies één validatiefout, op het veld Prijs.
            Assert.False(isGeldig);
            var fout = Assert.Single(fouten);
            Assert.Contains(nameof(Product.Prijs), fout.MemberNames);
        }
    }
}
