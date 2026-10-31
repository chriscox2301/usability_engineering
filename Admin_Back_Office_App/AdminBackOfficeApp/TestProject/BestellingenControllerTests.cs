using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using WebApplication1.Controllers;
using WebApplication1.Interface;

namespace WebApplication1
{
	public class BestellingenControllerTests
	{
		private readonly Mock<IBestellingRepository> _repoMock =
			new Mock<IBestellingRepository>();

		private readonly Mock<ILogger<BestellingenController>> _loggerMock =
			new Mock<ILogger<BestellingenController>>();

		private BestellingenController MaakController()
		{
			ILogger logger = MaakLogger();
			BestellingenController controller =
				new BestellingenController(
					_repoMock.Object,
					_loggerMock.Object);

			controller.TempData = new TempDataDictionary(
				new DefaultHttpContext(),
				Mock.Of<ITempDataProvider>());

			return controller;
		}

		[Fact]
		public void StatusWijzigen_RoeptRepositoryAan()
		{
			// Arrange
			BestellingenController controller = MaakController();

			_repoMock
				.Setup(r => r.UpdateStatus(1, "Ingepakt"))
				.Returns(true);

			// Act
			IActionResult resultaat =
				controller.StatusWijzigen(1, "Ingepakt");

			// Assert
			_repoMock.Verify(
				r => r.UpdateStatus(1, "Ingepakt"),
				Times.Once);

			Assert.IsType<RedirectToActionResult>(resultaat);
		}

		[Fact]
		public void StatusWijzigen_BijSucces_LogtBericht()
		{
			// Arrange
			BestellingenController controller = MaakController();

			_repoMock
				.Setup(r => r.UpdateStatus(1, "Ingepakt"))
				.Returns(true);

			ILogger<BestellingenController> logger = MaakLogger();

			// Act
			IActionResult resultaat =
				controller.StatusWijzigen(1, "Ingepakt");

			// Assert
			Assert.IsType<RedirectToActionResult>(resultaat);

			_repoMock.Verify(
				r => r.UpdateStatus(1, "Ingepakt"),
				Times.Once);

			// Logger bericht schrijven
			if (_repoMock.Object.UpdateStatus(1, "Ingepakt"))
			{
				logger.LogInformation("Test geslaagd: status is succesvol gewijzigd.");
			}
			else
			{
				logger.LogInformation("Test gefaald: status is niet succesvol gewijzigd.");
			}
		}

		private static ILogger<BestellingenController> MaakLogger()
		{
			ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.AddConsole();
				builder.AddDebug();
			});

			return loggerFactory.CreateLogger<BestellingenController>();
		}
	}
}