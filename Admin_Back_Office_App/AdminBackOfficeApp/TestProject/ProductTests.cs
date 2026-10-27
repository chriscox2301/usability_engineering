using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using WebApplication1.Controllers;
using WebApplication1.Data;
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

