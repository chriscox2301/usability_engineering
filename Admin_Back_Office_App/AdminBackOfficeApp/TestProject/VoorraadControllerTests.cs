using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using WebApplication1.Controllers;
using WebApplication1.Interface;

namespace WebApplication1
{
    public class VoorraadControllerTests
    {
        private readonly Mock<IProductRepository> _repoMock = new Mock<IProductRepository>();

        // Constuctor
        private VoorraadController MaakController()
        {
            VoorraadController controller = new VoorraadController(_repoMock.Object);
            // TempData is nodig omdat Aanpassen een melding zet.
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            return controller;
        }

        // Logger
        private static ILogger MaakLogger()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            return loggerFactory.CreateLogger<VoorraadControllerTests>();
        }


        [Fact]
        public void Aanpassen_NegatieveVoorraad_WordtNietOpgeslagen()
        {
            // Arrange
            VoorraadController controller = MaakController();
            ILogger logger = MaakLogger();
            int voorraad = -5;

            // Onthoud of de controller PasVoorraadAan aanroept, zodat we dat kunnen loggen.
            bool isOpgeslagen = false;
            _repoMock.Setup(r => r.PasVoorraadAan(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => isOpgeslagen = true);

            // Act
            IActionResult resultaat = controller.Aanpassen(id: 1, voorraad: voorraad);

            // Assert
            if (isOpgeslagen)
            {
                logger.LogError("{Test}: MISLUKT - voorraad {Voorraad} werd opgeslagen, maar had afgekeurd moeten worden.",
                    nameof(Aanpassen_NegatieveVoorraad_WordtNietOpgeslagen), voorraad);
            }
            else
            {
                logger.LogInformation("{Test}: GESLAAGD - voorraad {Voorraad} werd terecht niet opgeslagen.",
                    nameof(Aanpassen_NegatieveVoorraad_WordtNietOpgeslagen), voorraad);
            }

            // De repository is NIET aangeroepen, dus er is niets naar de database gegaan.
            _repoMock.Verify(r => r.PasVoorraadAan(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            Assert.IsType<RedirectToActionResult>(resultaat);
        }


        [Fact]
        public void Aanpassen_GeldigeVoorraad_WordtOpgeslagen()
        {
            // Arrange
            VoorraadController controller = MaakController();
            ILogger logger = MaakLogger();
            int voorraad = 20;

            // Onthoud of de controller PasVoorraadAan aanroept, zodat we dat kunnen loggen.
            bool isOpgeslagen = false;
            _repoMock.Setup(r => r.PasVoorraadAan(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => isOpgeslagen = true);

            // Act
            controller.Aanpassen(id: 1, voorraad: voorraad);

            // Assert
            if (isOpgeslagen)
            {
                logger.LogInformation("{Test}: GESLAAGD - voorraad {Voorraad} werd terecht opgeslagen.",
                    nameof(Aanpassen_GeldigeVoorraad_WordtOpgeslagen), voorraad);
            }
            else
            {
                logger.LogError("{Test}: MISLUKT - voorraad {Voorraad} werd niet opgeslagen, maar had opgeslagen moeten worden.",
                    nameof(Aanpassen_GeldigeVoorraad_WordtOpgeslagen), voorraad);
            }

            // De repository is precies één keer aangeroepen, met id 1 en voorraad 20.
            _repoMock.Verify(r => r.PasVoorraadAan(1, voorraad), Times.Once);
        }
    }
}
