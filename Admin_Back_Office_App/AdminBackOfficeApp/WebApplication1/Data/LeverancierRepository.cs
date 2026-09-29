using Microsoft.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    /* LeverancierRepository leest leveranciers uit SQL Server en zet rijen om naar Leverancier-models. */
    public class LeverancierRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        // Constructor ontvangt DatabaseConnection via Dependency Injection.
        public LeverancierRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Haalt alle leveranciers op en sorteert ze op naam.
        public List<Leverancier> GetAlleLeveranciers()
        {
            var leveranciers = new List<Leverancier>();

            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand("SELECT Id, Naam, Contactpersoon, Email, Telefoon, Status FROM Leveranciers ORDER BY Naam;", connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                leveranciers.Add(new Leverancier
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Naam = reader.GetString(reader.GetOrdinal("Naam")),
                    Contactpersoon = reader.GetString(reader.GetOrdinal("Contactpersoon")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Telefoon = reader.GetString(reader.GetOrdinal("Telefoon")),
                    Status = reader.GetString(reader.GetOrdinal("Status"))
                });
            }

            return leveranciers;
        }
    }
}
