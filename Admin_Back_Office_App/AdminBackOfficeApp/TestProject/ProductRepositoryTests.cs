using Microsoft.Extensions.Configuration;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1
{
    /*
        Integratietests voor ProductRepository.

        Deze tests praten met de echte database uit appsettings.json (LocalDB, MatrixIncDb).
        Ze controleren of ProductRepository, de stored procedures en SQL Server goed samenwerken:
        een product dat we opslaan, moeten we daarna ook weer precies zo kunnen ophalen.

        Let op: deze tests slagen alleen als de database bereikbaar is.
        Het testproduct wordt na afloop weer verwijderd, zodat de database schoon blijft.

        Elke test volgt het Arrange-Act-Assert patroon:
        - Arrange: de testdata klaarzetten.
        - Act:     de handeling uitvoeren die we testen.
        - Assert:  controleren of de uitkomst klopt.
    */
    public class ProductRepositoryTests
    {
        private readonly ProductRepository _repository;

        public ProductRepositoryTests()
        {
            // Dezelfde configuratie (appsettings.json) en DatabaseConnection als de applicatie gebruikt.
            var configuratie = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _repository = new ProductRepository(new DatabaseConnection(configuratie));
        }

        // Product opslaan en ophalen - Opgehaald product heeft dezelfde gegevens
        [Fact]
        public void ProductOpslaanEnOphalen_ProductHeeftDezelfdeGegevens()
        {
            // Arrange: een geldig Concept-product met een uniek artikelnummer,
            // zodat we het na het opslaan terug kunnen vinden. Categorie en leverancier bestaan al in de database.
            var artikelnummer = $"TEST-{DateTime.Now:yyyyMMddHHmmssfff}";
            var product = new Product
            {
                Naam = "Integratietest kogellager",
                Omschrijving = "Testproduct, aangemaakt door een integratietest.",
                Artikelnummer = artikelnummer,
                Merk = "SKF",
                Categorie = _repository.GetCategorieNamen().First(),
                Leverancier = _repository.GetLeverancierNamen().First(),
                Prijs = 12.50m,
                Voorraad = 10,
                Status = "Concept"
            };

            Product? opgeslagenProduct = null;

            try
            {
                // Act: het product opslaan en daarna weer ophalen uit de database.
                _repository.VoegProductToe(product);

                // VoegProductToe geeft geen Id terug, dus zoeken we het product op via het unieke artikelnummer.
                opgeslagenProduct = _repository.GetAlleProducten().SingleOrDefault(p => p.Artikelnummer == artikelnummer);
                Assert.NotNull(opgeslagenProduct);

                var opgehaaldProduct = _repository.GetProductById(opgeslagenProduct.Id);

                // Assert: het opgehaalde product bestaat en heeft dezelfde gegevens als het opgeslagen product.
                Assert.NotNull(opgehaaldProduct);
                Assert.Equal(product.Naam, opgehaaldProduct.Naam);
                Assert.Equal(product.Omschrijving, opgehaaldProduct.Omschrijving);
                Assert.Equal(product.Artikelnummer, opgehaaldProduct.Artikelnummer);
                Assert.Equal(product.Merk, opgehaaldProduct.Merk);
                Assert.Equal(product.Categorie, opgehaaldProduct.Categorie);
                Assert.Equal(product.Leverancier, opgehaaldProduct.Leverancier);
                Assert.Equal(product.Prijs, opgehaaldProduct.Prijs);
                Assert.Equal(product.Voorraad, opgehaaldProduct.Voorraad);
                Assert.Equal(product.Status, opgehaaldProduct.Status);
            }
            finally
            {
                // Opruimen: het testproduct weer verwijderen, ook als een Assert hierboven faalt.
                if (opgeslagenProduct != null)
                {
                    _repository.VerwijderConceptProduct(opgeslagenProduct.Id);
                }
            }
        }

        // Product ophalen - Id bestaat niet - Geeft null terug
        [Fact]
        public void ProductOphalen_OnbekendId_GeeftNull()
        {
            // Arrange: een Id dat niet in de database voorkomt.
            var onbekendId = -1;

            // Act: het product ophalen.
            var product = _repository.GetProductById(onbekendId);

            // Assert: er is geen product gevonden.
            Assert.Null(product);
        }
    }
}