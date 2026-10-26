using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class DatabaseTests
    {
        // Integratietest: controleert of de applicatie echt verbinding kan maken met de database.
        // Let op: deze test slaagt alleen als de database uit appsettings.json (LocalDB) bereikbaar is.
        [Fact]
        public void DatabaseConnectie_Werkt()
        {
            // Arrange: dezelfde configuratie (appsettings.json) en DatabaseConnection als de applicatie gebruikt.
            var configuratie = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            var databaseConnection = new DatabaseConnection(configuratie);

            // ILoggerFactory maakt loggers aan; hier loggen we naar de console en naar het Debug-venster.
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
            var logger = loggerFactory.CreateLogger<DatabaseTests>();


            // Act: de verbinding openen en een eenvoudige query uitvoeren.
            using var verbinding = databaseConnection.MaakVerbinding();
            logger.LogInformation("Verbinding maken met database {Database} op server {Server}...", verbinding.Database, verbinding.DataSource);
            object? resultaat;

            try
            {
                verbinding.Open();
                using var command = new SqlCommand("SELECT 1", verbinding);
                resultaat = command.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                // Mislukt de verbinding, dan loggen we de fout en laten we de test falen.
                logger.LogInformation(ex, "Databaseconnectie werkt niet: {Reden}", ex.Message);
                throw;
            }

            logger.LogInformation("Databaseconnectie werkt: verbonden met database {Database} op server {Server}.", verbinding.Database, verbinding.DataSource);


            // Assert: de verbinding is open en de database geeft antwoord op de query.
            Assert.Equal(ConnectionState.Open, verbinding.State);
            Assert.Equal(1, resultaat);
        }

        // Integratietest: test dezelfde databasecontrole die Program.cs bij het opstarten uitvoert.
        [Fact]
        public void ControleerDatabaseConnectie_DatabaseBereikbaar()
        {
            // Arrange: dezelfde configuratie (appsettings.json) en DatabaseConnection als de applicatie gebruikt.
            var configuratie = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var databaseConnection = new DatabaseConnection(configuratie);
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
            var logger = loggerFactory.CreateLogger<DatabaseTests>();


            // Act: de controle uit Program.cs uitvoeren.
            var databaseWerkt = WebApplication1.Program.ControleerDatabaseConnectie(databaseConnection, logger);


            // Assert: de database is bereikbaar.
            Assert.True(databaseWerkt);
        }
    }
}
