using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using WebApplication1.Models;
using Xunit.Abstractions;

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
        // Hulpmethode om te loggen.
        private static ILogger MaakLogger()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            return loggerFactory.CreateLogger<DatabaseTests>();
        }
        
        // Geldig product
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

        // Product aanmaken - Geldige productnaam en prijs - Product wordt aangemaakt
        [Fact]
        public void Product_GeldigePrijs()
        {
            // Arrange: een product met een geldige naam en een geldige prijs.
            Product product = ValidProduct();
            product.Prijs = 12.50m;
            ILogger logger = MaakLogger();

            // Act: het product valideren zoals ASP.NET Core dat doet bij het opslaan.
            bool isGeldig = IsValid(product);

            // Assert: het product is geldig, dus het wordt aangemaakt.
            Assert.True(isGeldig);
            if (isGeldig)
            {
                logger.LogInformation("Product is geldig: Naam='{Naam}', Prijs={Prijs}", product.Naam, product.Prijs);

            }
            else
            {
                logger.LogError("Product is ongeldig: Naam='{Naam}', Prijs={Prijs}", product.Naam, product.Prijs);


            }
        }
        
        [Fact]
        public void Product_NegatievePrijs()
        {
            // Arrange: een product met een geldige naam, maar een negatieve prijs.
            Product product = ValidProduct();
            product.Prijs = -5.00m;
            ILogger logger = MaakLogger();

            // Act: het product valideren zoals ASP.NET Core dat doet bij het opslaan.
            bool isGeldig = IsValid(product);

            // Assert: het product is ongeldig, dus het wordt niet aangemaakt.
            Assert.False(isGeldig);
            if (isGeldig)
            {
                logger.LogError("Product is geldig: Naam='{Naam}', Prijs={Prijs}", product.Naam, product.Prijs);
            }
            else
            {
                logger.LogInformation("Product is ongeldig: Naam='{Naam}', Prijs={Prijs}", product.Naam, product.Prijs);
            }
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

