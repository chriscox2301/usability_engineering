using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Controllers;
using WebApplication1.Interface;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1
{
    public class ProductZoekenTests
    {

         private readonly ILogger<ProductenController> logger1;

    public ProductZoekenTests()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder.AddConsole()
        );

        logger1 = loggerFactory.CreateLogger<ProductenController>();
    }


        [Fact]
        public void BestaandeProductnaam_GeeftProductTerug()
        {
            // Arrange: maak een bestaand testproduct.

       
        Product product = new Product
            {
                Id = 1,
                Naam = "Kogellager 6204",
                Artikelnummer = "KL-6204",
                Merk = "SKF",
                Categorie = "Lagers",
                Omschrijving = "Testproduct",
                Prijs = 12.50m,
                Voorraad = 10,
                Status = "Actief"
            };

            // Mock: boots de repository na, zonder database.
            Mock<IProductRepository> mockRepository = new Mock<IProductRepository>();

            mockRepository
                .Setup(r => r.GetAlleProducten())
                .Returns(new List<Product> { product });

            mockRepository
                .Setup(r => r.GetCategorieNamen())
                .Returns(new List<string> { "Lagers" });

            ProductenController controller = new ProductenController(
                mockRepository.Object, logger1);

            // Act: zoek op de naam van het bestaande product.
            IActionResult resultaat = controller.Index(
                "Kogellager 6204",
                null,
                "Actief",
                null);

            // Assert: controleer of het product gevonden is.
            ViewResult viewResult = Assert.IsType<ViewResult>(resultaat);

            List<Product> producten = Assert.IsAssignableFrom<List<Product>>(
            viewResult.Model);

            Product gevondenProduct = Assert.Single(producten);

            Assert.Equal("Kogellager 6204", gevondenProduct.Naam);

            // Controleer of de repository is aangeroepen.
            mockRepository.Verify(
                r => r.GetAlleProducten(),
                Times.Once);
        }


        [Fact]
        public void NietBestaandeProductnaam_GeeftGeenProductTerug()
        {
            // Arrange: maak een mock repository met een bestaand product.
            Mock<IProductRepository> mockRepository =
                new Mock<IProductRepository>();

            Product bestaandProduct = new Product
            {
                Id = 1,
                Naam = "Kogellager 6204",
                Artikelnummer = "KL-6204",
                Merk = "SKF",
                Categorie = "Lagers",
                Omschrijving = "Testproduct",
                Prijs = 12.50m,
                Voorraad = 10,
                Status = "Actief"
            };

            mockRepository
                .Setup(r => r.GetAlleProducten())
                .Returns(new List<Product> { bestaandProduct });

            mockRepository
                .Setup(r => r.GetCategorieNamen())
                .Returns(new List<string> { "Lagers" });

            ProductenController controller =
                new ProductenController(mockRepository.Object, logger1);

            // Act: zoek naar een product dat niet bestaat.
            IActionResult resultaat = controller.Index(
                "NietBestaandProduct",
                null,
                "Actief",
                null);

            // Assert: controleer dat er geen producten gevonden zijn.
            ViewResult viewResult = Assert.IsType<ViewResult>(resultaat);

            List<Product> producten =
                Assert.IsAssignableFrom<List<Product>>(viewResult.Model);

            Assert.Empty(producten);

            mockRepository.Verify(
                r => r.GetAlleProducten(),
                Times.Once);
        }

    }
}