using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer.Repositories
{
    /*
        ProductRepository gebruikt nu ADO.NET en dezelfde MatrixIncDb database als de MVC back-office app.

        Belangrijk:
        - Geen Entity Framework.
        - Geen DbContext.
        - Geen migrations.
        - Producten komen uit SQL Server tabel Producten, gekoppeld aan Categorieen en Leveranciers.
    */
    public class ProductRepository : IProductRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public ProductRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public IEnumerable<Product> HaalAlleProductenOp()
        {
            var producten = new List<Product>();

            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand(@"
                SELECT p.Id, p.Naam, p.Omschrijving, p.Artikelnummer, p.Merk,
                       c.Naam AS Categorie, p.Prijs, p.Voorraad, p.Status, p.Icoon,
                       COALESCE(l.Naam, '') AS Leverancier, p.LaatstGewijzigd
                FROM Producten p
                INNER JOIN Categorieen c ON p.CategorieId = c.Id
                LEFT JOIN Leveranciers l ON p.LeverancierId = l.Id
                WHERE p.Status = 'Actief'
                ORDER BY p.Id;", connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                producten.Add(MapProduct(reader));
            }

            return producten;
        }

        public Product? HaalProductOpMetId(int id)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand(@"
                SELECT p.Id, p.Naam, p.Omschrijving, p.Artikelnummer, p.Merk,
                       c.Naam AS Categorie, p.Prijs, p.Voorraad, p.Status, p.Icoon,
                       COALESCE(l.Naam, '') AS Leverancier, p.LaatstGewijzigd
                FROM Producten p
                INNER JOIN Categorieen c ON p.CategorieId = c.Id
                LEFT JOIN Leveranciers l ON p.LeverancierId = l.Id
                WHERE p.Id = @Id AND p.Status = 'Actief';", connection);

            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            using var reader = command.ExecuteReader();

            return reader.Read() ? MapProduct(reader) : null;
        }

        private static Product MapProduct(SqlDataReader reader)
        {
            var categorie = reader.GetString(reader.GetOrdinal("Categorie"));
            var artikelnummer = reader.GetString(reader.GetOrdinal("Artikelnummer"));

            return new Product
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Naam = reader.GetString(reader.GetOrdinal("Naam")),
                Omschrijving = reader.GetString(reader.GetOrdinal("Omschrijving")),
                Artikelnummer = artikelnummer,
                Merk = reader.GetString(reader.GetOrdinal("Merk")),
                Categorie = categorie,
                Prijs = reader.GetDecimal(reader.GetOrdinal("Prijs")),
                Voorraad = reader.GetInt32(reader.GetOrdinal("Voorraad")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Icoon = reader.GetString(reader.GetOrdinal("Icoon")),
                Leverancier = reader.GetString(reader.GetOrdinal("Leverancier")),
                LaatstGewijzigd = reader.GetDateTime(reader.GetOrdinal("LaatstGewijzigd")).ToString("dd-MM-yyyy HH:mm"),
                Materiaal = categorie,
                Afmeting = artikelnummer,
                Levertijd = reader.GetInt32(reader.GetOrdinal("Voorraad")) > 0 ? "2 werkdagen" : "Niet op voorraad"
            };
        }
    }
}
