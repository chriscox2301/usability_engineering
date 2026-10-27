using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApplication1.Data;

namespace WebApplication1
{
    /*
        Integratietests voor de databaseverbinding.

        Deze tests praten met de echte database uit appsettings.json (LocalDB).
        Ze slagen dus alleen als die database bereikbaar is.

        Elke test volgt het Arrange-Act-Assert patroon:
        - Arrange: alles klaarzetten wat de test nodig heeft.
        - Act:     de handeling uitvoeren die we testen.
        - Assert:  controleren of de uitkomst klopt.
    */
    public class DatabaseTests
    {
        // Test 1: kunnen we zelf een verbinding openen en een query uitvoeren?
        [Fact]
        public void DatabaseConnectie_Query_Werkt()
        {
            // Arrange
            DatabaseConnection databaseConnection = MaakDatabaseConnection();
            using SqlConnection verbinding = databaseConnection.MaakVerbinding();
            ILogger logger = MaakLogger();

            // Act
            verbinding.Open();
            using SqlCommand command = new SqlCommand("SELECT 1", verbinding);
            object? resultaat = command.ExecuteScalar();

            // Assert
            // We loggen eerst de echte waarde, zodat je die ook ziet als de assert faalt.
            logger.LogInformation("Assert 1: status van de verbinding is {Status} (verwacht: Open).", verbinding.State);
            Assert.Equal(ConnectionState.Open, verbinding.State);

            logger.LogInformation("Assert 2: resultaat van SELECT 1 is {Resultaat} (verwacht: 1).", resultaat);
            Assert.Equal(1, resultaat);
        }

        // Test 2: werkt de controle die Program.cs bij het opstarten uitvoert?
        [Fact]
        public void ControleerDatabaseConnectie_DatabaseBereikbaar()
        {
            // Arrange
            DatabaseConnection databaseConnection = MaakDatabaseConnection();
            ILogger logger = MaakLogger();

            // Act
            bool databaseWerkt = Program.ControleerDatabaseConnectie(databaseConnection, logger);

            // Assert
            Assert.True(databaseWerkt);
        }

        // Leest appsettings.json in en maakt daarmee een DatabaseConnection,
        // precies zoals de applicatie dat zelf ook doet.
        private static DatabaseConnection MaakDatabaseConnection()
        {
            IConfigurationRoot configuratie = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            return new DatabaseConnection(configuratie);
        }

        // Maakt een logger die berichten naar de console en het Debug-venster schrijft.
        private static ILogger MaakLogger()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            return loggerFactory.CreateLogger<DatabaseTests>();
        }
    }
}
