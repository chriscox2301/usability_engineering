using Microsoft.Data.SqlClient;

namespace WebApplication1.Data
{
    /*
        DatabaseConnection is de centrale plek voor databaseverbindingen.

        Waarom apart bestand?
        - Controllers hoeven geen connection string te kennen.
        - Repositories kunnen allemaal dezelfde manier gebruiken om een SqlConnection te maken.
        - Als de servernaam wijzigt, pas je alleen appsettings.json aan.
    */
    public class DatabaseConnection
    {
        // Private veld waarin de connection string uit appsettings.json wordt opgeslagen.
        private readonly string _connectionString;

        /*
            Constructor:
            IConfiguration wordt door ASP.NET Core automatisch geïnjecteerd.
            configuration.GetConnectionString("DefaultConnection") leest:
            appsettings.json -> ConnectionStrings -> DefaultConnection.
        */
        public DatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' ontbreekt in appsettings.json.");
        }

        /*
            Maakt een nieuw SqlConnection-object.
            Let op: deze methode opent de verbinding nog niet.
            Repositories doen daarna zelf connection.Open() vlak voordat ze een query uitvoeren.
        */
        public SqlConnection MaakVerbinding()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
