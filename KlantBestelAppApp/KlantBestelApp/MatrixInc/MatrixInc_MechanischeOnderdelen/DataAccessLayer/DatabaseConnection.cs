using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer
{
    /*
        DatabaseConnection is dezelfde SQL Server-aanpak als in de MVC back-office applicatie.

        Relatie met de projecten:
        - De klant bestelapp en de medewerkers/back-office app gebruiken allebei MatrixIncDb.
        - De connection string staat in appsettings.json onder ConnectionStrings:DefaultConnection.
        - Repositories maken via deze klasse een SqlConnection en voeren daarna ADO.NET queries uit.
    */
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' ontbreekt in appsettings.json.");
        }

        public SqlConnection MaakVerbinding()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
