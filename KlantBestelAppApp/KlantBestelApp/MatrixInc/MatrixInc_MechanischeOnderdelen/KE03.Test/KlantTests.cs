using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using KE03_INTDEV_SE_1_Base.Pages.Bestellingen;
using KE03_INTDEV_SE_1_Base.Pages.Producten;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace KE03.Test
{
    /*
        Unit tests voor de Klant-rol: product zoeken (TC09, TC10) en product bestellen (TC11, TC12).

        - De database wordt met Moq nagebootst, zodat de tests geen echte database nodig hebben.
        - Elke test volgt het Arrange-Act-Assert patroon:
            Arrange: de testdata klaarzetten.
            Act:     de handeling uitvoeren die we testen.
            Assert:  controleren of de uitkomst klopt.
    */
    public class KlantTests
    {
        // Het product dat in de nep-database staat.
        private Product kogellager = new Product
        {
            Id = 1,
            Naam = "Kogellager 6204",
            Omschrijving = "Diepgroefkogellager",
            Artikelnummer = "KL-6204",
            Merk = "SKF",
            Categorie = "Lagers",
            Prijs = 12.50m,
            Voorraad = 10
        };

        // De winkelmand wordt in de Session bewaard. Hier maken we een lege Session,
        // net zoals de website dat doet wanneer een klant binnenkomt.
        private ISession sessie = new DistributedSession(
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())),
            "test", TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(1), () => true, NullLoggerFactory.Instance, true);

        // Geeft een pagina toegang tot de Session en TempData, net als in de echte website.
        private void KoppelSessie(PageModel pagina)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Session = sessie;
            pagina.PageContext = new PageContext { HttpContext = httpContext };
            pagina.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        // TC09: Product zoeken - Bestaande productnaam - Product wordt gevonden
        [Fact]
        public void Zoeken_BestaandeProductnaam_ProductWordtGevonden()
        {
            // Arrange: de nep-database bevat één kogellager.
            Mock<IProductRepository> productRepo = new Mock<IProductRepository>();
            productRepo.Setup(r => r.HaalAlleProductenOp()).Returns(new List<Product> { kogellager });
            OverzichtModel pagina = new OverzichtModel(productRepo.Object);

            // Act: zoeken op "Kogellager".
            pagina.OnGet(categorie: null, zoekterm: "Kogellager");

            // Assert: er is precies één product gevonden, en dat is de kogellager.
            Assert.Single(pagina.Producten);
            Assert.Equal("Kogellager 6204", pagina.Producten[0].Naam);
        }

        // TC10: Product zoeken - Niet-bestaande productnaam - Geen product gevonden
        [Fact]
        public void Zoeken_NietBestaandeProductnaam_GeenProductGevonden()
        {
            // Arrange: de nep-database bevat één kogellager.
            Mock<IProductRepository> productRepo = new Mock<IProductRepository>();
            productRepo.Setup(r => r.HaalAlleProductenOp()).Returns(new List<Product> { kogellager });
            OverzichtModel pagina = new OverzichtModel(productRepo.Object);

            // Act: zoeken op een product dat niet bestaat.
            pagina.OnGet(categorie: null, zoekterm: "Tandwiel");

            // Assert: er is niets gevonden.
            Assert.Empty(pagina.Producten);
        }

        // TC11: Product bestellen - Geldig product + geldige bestelling - Bestelling wordt aangemaakt
        [Fact]
        public void Bestellen_GeldigProductEnAantal_BestellingWordtAangemaakt()
        {
            // Arrange: de klant legt 2 kogellagers in de winkelmand.
            Mock<IProductRepository> productRepo = new Mock<IProductRepository>();
            productRepo.Setup(r => r.HaalProductOpMetId(1)).Returns(kogellager);

            DetailModel productPagina = new DetailModel(productRepo.Object);
            KoppelSessie(productPagina);
            productPagina.ProductId = 1;
            productPagina.Aantal = 2;
            productPagina.OnPost();

            // De nep-database geeft bij het opslaan het ordernummer "ORD-TEST" terug.
            Mock<IBestellingRepository> bestellingRepo = new Mock<IBestellingRepository>();
            bestellingRepo.Setup(r => r.PlaatsBestelling(It.IsAny<GeplaatsteBestelling>())).Returns("ORD-TEST");

            PlaatsenModel bestelPagina = new PlaatsenModel(bestellingRepo.Object);
            KoppelSessie(bestelPagina);

            // Act: de klant plaatst de bestelling.
            bestelPagina.OnPost();

            // Assert: de bestelling is één keer opgeslagen en de klant krijgt het ordernummer te zien.
            bestellingRepo.Verify(r => r.PlaatsBestelling(It.IsAny<GeplaatsteBestelling>()), Times.Once);
            Assert.True(bestelPagina.Besteld);
            Assert.Equal("ORD-TEST", bestelPagina.LaatsteOrdernummer);
        }

        // TC12: Product bestellen - Ongeldige hoeveelheid - Validatiefout
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Bestellen_OngeldigAantal_GeeftValidatiefout(int aantal)
        {
            // Arrange: de klant vult een ongeldig aantal in.
            Mock<IProductRepository> productRepo = new Mock<IProductRepository>();
            productRepo.Setup(r => r.HaalProductOpMetId(1)).Returns(kogellager);

            DetailModel productPagina = new DetailModel(productRepo.Object);
            KoppelSessie(productPagina);
            productPagina.ProductId = 1;
            productPagina.Aantal = aantal;

            // Act: de klant klikt op "Toevoegen aan winkelmand".
            productPagina.OnPost();

            // Assert: de pagina moet een validatiefout geven.
            Assert.False(productPagina.ModelState.IsValid);
        }
    }
}
