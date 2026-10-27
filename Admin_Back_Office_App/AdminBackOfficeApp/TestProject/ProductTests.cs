using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1
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
    public class ProductTests
    {
        // Product aanmaken - Geldige productnaam en prijs - Product wordt aangemaakt
        [Fact]
        public void ProductAanmaken_GeldigeNaamEnPrijs_ProductWordtAangemaakt()
        {
            // Arrange: een product met een geldige naam en een geldige prijs.
            Product product = new Product
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
            List<ValidationResult> fouten = new List<ValidationResult>();
            ValidationContext context = new ValidationContext(product);

            // Act: het product valideren zoals ASP.NET Core dat doet bij het opslaan.
            bool isGeldig = Validator.TryValidateObject(product, context, fouten, validateAllProperties: true);

            // Assert: het product is geldig, dus het wordt aangemaakt.
            Assert.True(isGeldig);
            Assert.Empty(fouten);
        }

        // Product aanmaken - Negatieve prijs - Validatiefout
        [Fact]
        public void ProductAanmaken_NegatievePrijs_Validatiefout()
        {
            // Arrange: een product dat verder geldig is, maar een negatieve prijs heeft.
            Product product = new Product
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
            List<ValidationResult> fouten = new List<ValidationResult>();
            ValidationContext context = new ValidationContext(product);

            // Act: het product valideren zoals ASP.NET Core dat doet bij het opslaan.
            bool isGeldig = Validator.TryValidateObject(product, context, fouten, validateAllProperties: true);

            // Assert: het product is ongeldig en er is precies één validatiefout, op het veld Prijs.
            Assert.False(isGeldig);
            ValidationResult fout = Assert.Single(fouten);
            Assert.Contains(nameof(Product.Prijs), fout.MemberNames);
        }

        private Product ValidProduct()
        {
            return new Product
            {
                Naam = "Kogellager 6204",
                Omschrijving = "Diepgroefkogellager",
                Artikelnummer = "KL-6204",
                Merk = "SKF",
                Categorie = "Lagers",
                Prijs = 12.99m,
                Voorraad = 10
            };
        }

        // Controleert de regels uit Product.cs en geeft true terug als het product goed is, false als er een fout in zit.
        private bool IsValid(Product product)
        {
            ValidationContext context = new ValidationContext(product);
            List<ValidationResult> mistakes = new List<ValidationResult>();
            return Validator.TryValidateObject(product, context, mistakes, true);
        }
        // Unit tests Chris ----------------------------------------------------------------------------------------------------
        // TC01
        [Fact]
        public void ValidNameAndPrice_ProductIsValid()
        {
            // Arrange
            Product product = ValidProduct();
            // Act
            // Assert
            Assert.True(IsValid(product));
        }
        // TC02
        [Fact]
        public void NegativePrice_ReturnsFalse()
        {
            // Arrange
            Product product = ValidProduct();
            // Act
            product.Prijs = -1.00m;
            // Assert
            Assert.False(IsValid(product));
        }

        // TC03
        [Fact]
        public void PriceZero_ReturnsFalse()
        {
            // Arrange
            Product product = ValidProduct();
            // Act
            product.Prijs = 0.00m;
            // Assert
            Assert.False(IsValid(product));
        }

        // TC04
        [Fact]
        public void MaxVoorraad_IsValid()
        {
            // Arrange
            Product product = ValidProduct();
            // Act
            product.Voorraad = 1000;
            // Assert
            Assert.True(IsValid(product));
        }

        // TC05
        [Fact]
        public void ToHighPrice_ReturnsError()
        {
            // Arrange
            Product product = ValidProduct();
            // Act
            product.Prijs = 100000000m;
            // Assert
            Assert.False(IsValid(product));
        }

        // Unit tests Milan --------------------------------------------------------------------------------------

        // TC06: Controleren of letters in een prijsveld worden afgewezen.
        [Fact]
        public void LettersInPrijs_GeeftValidatiefout()
        {
            // Arrange
            string invoer = "abc";

            // Act
            bool resultaat = decimal.TryParse(invoer, out decimal prijs);

            // Assert
            Assert.False(resultaat);
        }

        // TC07: Controleren of een prijs met euroteken en spatie correct wordt verwerkt.
        [Fact]
        public void EurotekenEnSpatieInPrijs_WordtCorrectVerwerkt()
        {
            // Arrange
            string invoer = "€ 12,50";

            // Act
            bool resultaat = decimal.TryParse(
                invoer,
                System.Globalization.NumberStyles.Currency,
                new System.Globalization.CultureInfo("nl-NL"),
                out decimal prijs);

            // Assert
            Assert.True(resultaat);
            Assert.Equal(12.50m, prijs);
        }
    }
}

