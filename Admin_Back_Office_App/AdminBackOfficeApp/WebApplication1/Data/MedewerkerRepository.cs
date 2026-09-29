using Microsoft.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    /* MedewerkerRepository leest medewerkers uit SQL Server en zet rijen om naar Medewerker-models. */
    public class MedewerkerRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        // Constructor ontvangt DatabaseConnection via Dependency Injection.
        public MedewerkerRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Haalt alle medewerkers op en sorteert ze op naam.
        public List<Medewerker> GetAlleMedewerkers()
        {
            var medewerkers = new List<Medewerker>();

            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand("SELECT Id, Naam, Rol, Email, LaatstActief FROM Medewerkers ORDER BY Naam;", connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                medewerkers.Add(new Medewerker
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Naam = reader.GetString(reader.GetOrdinal("Naam")),
                    Rol = reader.GetString(reader.GetOrdinal("Rol")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    LaatstActief = reader.IsDBNull(reader.GetOrdinal("LaatstActief"))
                        ? "Onbekend"
                        : reader.GetDateTime(reader.GetOrdinal("LaatstActief")).ToString("dd-MM-yyyy")
                });
            }

            return medewerkers;
        }
    }
}
