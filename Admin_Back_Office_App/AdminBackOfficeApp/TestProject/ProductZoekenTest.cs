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

    //Product zoeken test bevat unit testen die gaan over een product zoeken in de admin applicatie.
    // De unit tests bevatten mocking en een logger zodat je precies ziet wat er gebeurd.
    

    //logger1 wordt aangemaakt zodat die mee gegeven kan worden aan de product controller.
    public class ProductZoekenTests
    {
        
         private readonly ILogger<ProductenController> logger1;

    public ProductZoekenTests()

            // De logger zelf wordt aangemaakt, zodat de logger gebruikt kan worden.
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder.AddConsole()
        );

        logger1 = loggerFactory.CreateLogger<ProductenController>();
    }

        //bij deze test wordt getest of de zoekfunctie een bestaand product kan vinden.
   
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

            // Mock: Geeft info aan de repository , zonder database.
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


            //logger geeft aan wat de zoek actie is.
            logger1.LogInformation("Product zoeken test: Zoekactie uitgevoerd. Resultaattype: {Resultaat}",
            resultaat.GetType().Name);

            // logt het resultaat van de zoekactie
            logger1.LogInformation("Product zoeken test: Zoekactie uitgevoerd. Resultaattype: {Resultaat}",
            resultaat.GetType().Name);


            // Assert: controleer of het product gevonden is.
            ViewResult viewResult = Assert.IsType<ViewResult>(resultaat);

            List<Product> producten = Assert.IsAssignableFrom<List<Product>>(
            viewResult.Model);


            //logger geeft aan hoeveel producten zijn gevonden
            logger1.LogInformation("Product zoeken test: Aantal gevonden producten: {Aantal}",producten.Count);

            Product gevondenProduct = Assert.Single(producten);

            Assert.Equal("Kogellager 6204", gevondenProduct.Naam);

            // Controleer of de repository een keer is aangeroepen.
            mockRepository.Verify(
                r => r.GetAlleProducten(),
                Times.Once);
        }


        [Fact]
        //Test dat zoekt naar een niet bestaand product en checkt of je ook niks terug krijgt
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

            //Logt dat er wordt gezocht naar een niet bestand product
            logger1.LogInformation("Niet bestaand product zoeken test: Start product zoeken. Zoekterm: {Zoekterm}","NietBestaandProduct");

            // Act: zoek naar een product dat niet bestaat.
            IActionResult resultaat = controller.Index(
                "NietBestaandProduct",
                null,
                "Actief",
                null);

            // Logt het resultaat van de zoekactie
            logger1.LogInformation("Niet bestaand product zoeken test: Zoekactie uitgevoerd. Resultaattype: {Resultaat}",
            resultaat.GetType().Name);

            // Assert: controleer dat er geen producten gevonden zijn.
            ViewResult viewResult = Assert.IsType<ViewResult>(resultaat);

            List<Product> producten =
                Assert.IsAssignableFrom<List<Product>>(viewResult.Model);

            //logt hoeveel producten zijn gevonden
            logger1.LogInformation("Niet bestaand product zoeken test: Aantal gevonden producten: {Aantal}",producten.Count);

            Assert.Empty(producten);

            mockRepository.Verify(
                r => r.GetAlleProducten(),
                Times.Once);
        }

    }
}