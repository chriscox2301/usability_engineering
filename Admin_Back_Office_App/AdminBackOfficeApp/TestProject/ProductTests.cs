using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using WebApplication1.Controllers;
using Microsoft.Extensions.Logging;
using WebApplication1.Models;
using Xunit.Abstractions;
using WebApplication1.Interface;

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

        private bool IsValid(Product product)
        {
            ValidationContext context = new ValidationContext(product);
            List<ValidationResult> mistakes = new List<ValidationResult>();
            return Validator.TryValidateObject(product, context, mistakes, true);
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
            if (isGeldig)
            {
                logger.LogInformation("{Test}: GESLAAGD - prijs {Prijs} werd terecht geaccepteerd.",
                    nameof(Product_GeldigePrijs), product.Prijs);
            }
            else
            {
                logger.LogError("{Test}: MISLUKT - prijs {Prijs} werd afgekeurd, maar had geaccepteerd moeten worden.",
                    nameof(Product_GeldigePrijs), product.Prijs);
            }

            Assert.True(isGeldig);
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
            if (isGeldig)
            {
                logger.LogError("{Test}: MISLUKT - prijs {Prijs} werd geaccepteerd, maar had afgekeurd moeten worden.",
                    nameof(Product_NegatievePrijs), product.Prijs);
            }
            else
            {
                logger.LogInformation("{Test}: GESLAAGD - prijs {Prijs} werd terecht afgekeurd.",
                    nameof(Product_NegatievePrijs), product.Prijs);
            }

            Assert.False(isGeldig);
        }

        [Fact]
        public void Product_Naam_Null_Waarde()
        {
            // Arrange: een product met een null-waarde voor de naam.
            Product product = ValidProduct();
            product.Naam = null!;
            ILogger logger = MaakLogger();

            // Act: het product valideren zoals ASP.NET Core dat doet bij het opslaan.
            bool isGeldig = IsValid(product);

            // Assert: het product is ongeldig, dus het wordt niet aangemaakt.
            if (isGeldig)
            {
                logger.LogError("{Test}: MISLUKT - naam null werd geaccepteerd, maar had afgekeurd moeten worden.",
                    nameof(Product_Naam_Null_Waarde));
            }
            else
            {
                logger.LogInformation("{Test}: GESLAAGD - naam null werd terecht afgekeurd.",
                    nameof(Product_Naam_Null_Waarde));
            }

            Assert.False(isGeldig);
        }

        [Fact]
        public void Product_Prijs_Nul_Waarde() // Nul waarde is 0 NIET NULL!
        {             
            // Arrange: een product met een null-waarde voor de prijs.
            Product product = ValidProduct();
            product.Prijs = 0m;
            ILogger logger = MaakLogger();

            // Act: het product valideren zoals ASP.NET Core dat doet bij het opslaan.
            bool isGeldig = IsValid(product);

            // Assert: het product is ongeldig, dus het wordt niet aangemaakt.
            if (isGeldig)
            {
                logger.LogError("{Test}: MISLUKT - prijs {Prijs} werd geaccepteerd, maar had afgekeurd moeten worden.",
                    nameof(Product_Prijs_Nul_Waarde), product.Prijs);
            }
            else
            {
                logger.LogInformation("{Test}: GESLAAGD - prijs {Prijs} werd terecht afgekeurd.",
                    nameof(Product_Prijs_Nul_Waarde), product.Prijs);
            }

            Assert.False(isGeldig);
        }
        
        // -----------------------------------------------------------------------------------------------------------------
        private readonly Mock<IProductRepository> _repoMock = new Mock<IProductRepository>();
        private Product ValideProduct() => new Product
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

        private ProductenController MaakController()
        {
            // Nep-database: nog geen producten, lege keuzelijsten.
            _repoMock.Setup(r => r.GetAlleProducten()).Returns(new List<Product>());
            _repoMock.Setup(r => r.GetCategorieNamen()).Returns(new List<string>());
            _repoMock.Setup(r => r.GetLeverancierNamen()).Returns(new List<string>());

            ProductenController controller = new ProductenController(_repoMock.Object);
            // TempData is nodig omdat Toevoegen een melding zet.
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            return controller;
        }

        // Controleert de regels uit Product.cs en geeft true terug als het product goed is, false als er een fout in zit.
        private static void Valideer(Controller controller, Product product)
        {
            List<ValidationResult> fouten = new List<ValidationResult>();
            Validator.TryValidateObject(product, new ValidationContext(product), fouten, true);
            foreach (ValidationResult fout in fouten)
                controller.ModelState.AddModelError(fout.MemberNames.FirstOrDefault() ?? "", fout.ErrorMessage!);
        }
        // Unit tests Chris ----------------------------------------------------------------------------------------------------
        // TC01
        [Fact]
        public void Toevoegen_GeldigProduct_WordtOpgeslagen()
        {
            // Arrange
            ProductenController controller = MaakController();
            Product product = ValideProduct();
            Valideer(controller, product);

            // Act
            var resultaat = controller.Toevoegen(product);

            // Assert
            _repoMock.Verify(r => r.VoegProductToe(product), Times.Once);
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(resultaat);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        
        [Theory]
        [InlineData(-1.00)]         // TC02
        [InlineData(0.00)]          // TC03
        [InlineData(100000000.00)]  // TC05
        public void Toevoegen_OngeldigProduct_WordtNietOpgeslagen(double prijs)
        {
            // Arrange
            ProductenController controller = MaakController();
            Product product = ValideProduct();
            product.Prijs = (decimal)prijs;
            Valideer(controller, product);

            // Act
            var resultaat = controller.Toevoegen(product);

            // Assert
            _repoMock.Verify(r => r.VoegProductToe(product), Times.Never);
            ViewResult view = Assert.IsType<ViewResult>(resultaat);
            Assert.Same(product, view.Model);
            Assert.True(controller.ModelState.ContainsKey(nameof(Product.Prijs)));
        }

        // TC04
        [Fact]
        public void Toevoegen_ArtikelnummerBestaatAl_WordtNietOpgeslagen()
        {
            // Arrange
            ProductenController controller = MaakController();
            _repoMock.Setup(r => r.GetAlleProducten())
                .Returns(new List<Product> { new Product { Artikelnummer = "KL-6204" } });
            Product product = ValideProduct();
            Valideer(controller, product);

            // Act
            var resultaat = controller.Toevoegen(product);

            // Assert
            _repoMock.Verify(r => r.VoegProductToe(It.IsAny<Product>()), Times.Never);
            Assert.IsType<ViewResult>(resultaat);
            Assert.True(controller.ModelState.ContainsKey(nameof(Product.Artikelnummer)));
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

