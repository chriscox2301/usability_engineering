using Microsoft.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    /*
        CategorieRepository leest categorieën uit SQL Server.
        Controllers gebruiken deze repository zodat SQL-code niet in controllers terechtkomt.
    */
    public class CategorieRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        // Constructor krijgt DatabaseConnection via Dependency Injection.
        public CategorieRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Geeft een List<Categorie> terug voor de categoriepagina.
        public List<Categorie> GetAlleCategorieen()
        {
            var categorieen = new List<Categorie>();

            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand(@"
                SELECT c.Id, c.Naam, c.Omschrijving, c.Icoon, COUNT(p.Id) AS AantalProducten
                FROM Categorieen c
                LEFT JOIN Producten p ON c.Id = p.CategorieId
                GROUP BY c.Id, c.Naam, c.Omschrijving, c.Icoon
                ORDER BY c.Naam;", connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                categorieen.Add(new Categorie
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Naam = reader.GetString(reader.GetOrdinal("Naam")),
                    Omschrijving = reader.GetString(reader.GetOrdinal("Omschrijving")),
                    Icoon = reader.GetString(reader.GetOrdinal("Icoon")),
                    AantalProducten = reader.GetInt32(reader.GetOrdinal("AantalProducten"))
                });
            }

            return categorieen;
        }

        // Hergebruikt GetAlleCategorieen en zoekt daarna één item in C#.
        public Categorie? GetCategorieById(int id)
        {
            return GetAlleCategorieen().FirstOrDefault(c => c.Id == id);
        }
    }
}
