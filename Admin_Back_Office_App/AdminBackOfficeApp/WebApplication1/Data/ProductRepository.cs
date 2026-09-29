using Microsoft.Data.SqlClient;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    /*
        ProductRepository is de datalaag voor producten.

        Technische rol:
        - Controllers vragen productdata aan deze repository.
        - Deze repository praat met SQL Server via Microsoft.Data.SqlClient.
        - SqlConnection opent de verbinding.
        - SqlCommand voert een SQL-query of stored procedure uit.
        - SqlDataReader leest resultaatrijen terug.
        - MapProduct() zet één SQL-rij om naar één Product-model.
    */
    public class ProductRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        // Constructor: krijgt DatabaseConnection via Dependency Injection en bewaart deze in een private veld.
        public ProductRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Haalt alle producten op via stored procedure sp_Producten_GetAll.
        public List<Product> GetAlleProducten()
        {
            // List<Product> is de C# verzameling die uiteindelijk naar de controller teruggaat.
            var producten = new List<Product>();

            // using var zorgt dat de databaseverbinding netjes gesloten en opgeruimd wordt na gebruik.
            using var connection = _databaseConnection.MaakVerbinding();

            // SqlCommand vertegenwoordigt de SQL-instructie die SQL Server moet uitvoeren.
            using var command = new SqlCommand("sp_Producten_GetAll", connection);

            // CommandType.StoredProcedure betekent dat de tekst hierboven de naam van een stored procedure is.
            command.CommandType = CommandType.StoredProcedure;

            // Open de fysieke verbinding met SQL Server.
            connection.Open();

            // ExecuteReader voert de stored procedure uit en geeft rijen terug.
            using var reader = command.ExecuteReader();

            // reader.Read() gaat rij voor rij door het resultaat heen.
            while (reader.Read())
            {
                // MapProduct maakt van de huidige SQL-rij een Product-object.
                producten.Add(MapProduct(reader));
            }

            return producten;
        }

        // Haalt één product op via id. Geeft null terug als het product niet bestaat.
        public Product? GetProductById(int id)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand("sp_Producten_GetById", connection);
            command.CommandType = CommandType.StoredProcedure;

            // Parameter voorkomt string-concatenatie in SQL en beperkt SQL-injection risico.
            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            using var reader = command.ExecuteReader();

            // Als er een rij is, map die naar Product. Anders return null.
            return reader.Read() ? MapProduct(reader) : null;
        }

        // Voegt een product toe via stored procedure sp_Producten_Insert.
        public void VoegProductToe(Product product)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand("sp_Producten_Insert", connection);
            command.CommandType = CommandType.StoredProcedure;

            // includeId=false omdat SQL Server zelf een nieuw Id maakt via IDENTITY.
            VulProductParameters(command, product, false);

            connection.Open();

            // ExecuteNonQuery wordt gebruikt voor INSERT, UPDATE en DELETE zonder resultaatset.
            command.ExecuteNonQuery();
        }

        // Wijzigt een bestaand product via stored procedure sp_Producten_Update.
        public void WijzigProduct(Product product)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand("sp_Producten_Update", connection);
            command.CommandType = CommandType.StoredProcedure;

            // includeId=true omdat SQL Server moet weten welk product aangepast moet worden.
            VulProductParameters(command, product, true);

            connection.Open();
            command.ExecuteNonQuery();
        }

        // Zet een product op Inactief via stored procedure sp_Producten_Deactivate.
        public void DeactiveerProduct(int id)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand("sp_Producten_Deactivate", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        // Past alleen de voorraad van één product aan.
        public void PasVoorraadAan(int id, int voorraad)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand("sp_Producten_UpdateVoorraad", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Voorraad", voorraad);

            connection.Open();
            command.ExecuteNonQuery();
        }

        // Leest categorienamen voor dropdowns in productformulieren.
        public List<string> GetCategorieNamen()
        {
            var categorieen = new List<string>();
            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand("SELECT Naam FROM Categorieen ORDER BY Naam", connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                // GetString(0) leest de eerste kolom uit de SELECT, dus Naam.
                categorieen.Add(reader.GetString(0));
            }

            return categorieen;
        }

        // Leest actieve leveranciersnamen voor dropdowns in productformulieren.
        public List<string> GetLeverancierNamen()
        {
            var leveranciers = new List<string>();
            using var connection = _databaseConnection.MaakVerbinding();
            using var command = new SqlCommand("SELECT Naam FROM Leveranciers WHERE Status = 'Actief' ORDER BY Naam", connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                leveranciers.Add(reader.GetString(0));
            }

            return leveranciers;
        }

        // Zet de huidige rij van SqlDataReader om naar een Product-object.
        private static Product MapProduct(SqlDataReader reader)
        {
            return new Product
            {
                // GetOrdinal zoekt het kolomnummer op basis van de kolomnaam.
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Naam = reader.GetString(reader.GetOrdinal("Naam")),
                Omschrijving = reader.GetString(reader.GetOrdinal("Omschrijving")),
                Artikelnummer = reader.GetString(reader.GetOrdinal("Artikelnummer")),
                Merk = reader.GetString(reader.GetOrdinal("Merk")),
                Categorie = reader.GetString(reader.GetOrdinal("Categorie")),
                Prijs = reader.GetDecimal(reader.GetOrdinal("Prijs")),
                Voorraad = reader.GetInt32(reader.GetOrdinal("Voorraad")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Icoon = reader.GetString(reader.GetOrdinal("Icoon")),
                Leverancier = reader.GetString(reader.GetOrdinal("Leverancier")),
                LaatstGewijzigd = reader.GetDateTime(reader.GetOrdinal("LaatstGewijzigd")).ToString("dd-MM-yyyy HH:mm")
            };
        }

        // Vult SqlCommand parameters voor INSERT en UPDATE.
        private static void VulProductParameters(SqlCommand command, Product product, bool includeId)
        {
            // Bij update moet @Id mee; bij insert niet, omdat SQL Server zelf een Id maakt.
            if (includeId)
            {
                command.Parameters.AddWithValue("@Id", product.Id);
            }

            // AddWithValue koppelt C# waarden aan SQL parameters met dezelfde naam.
            command.Parameters.AddWithValue("@Naam", product.Naam);
            command.Parameters.AddWithValue("@Omschrijving", product.Omschrijving);
            command.Parameters.AddWithValue("@Artikelnummer", product.Artikelnummer);
            command.Parameters.AddWithValue("@Merk", product.Merk);
            command.Parameters.AddWithValue("@Categorie", product.Categorie);
            command.Parameters.AddWithValue("@Prijs", product.Prijs);
            command.Parameters.AddWithValue("@Voorraad", product.Voorraad);
            command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(product.Status) ? "Concept" : product.Status);
            command.Parameters.AddWithValue("@Icoon", string.IsNullOrWhiteSpace(product.Icoon) ? "package" : product.Icoon);
            command.Parameters.AddWithValue("@Leverancier", product.Leverancier ?? string.Empty);
        }
    }
}
