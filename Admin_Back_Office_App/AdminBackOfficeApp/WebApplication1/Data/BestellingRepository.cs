using Microsoft.Data.SqlClient;
using WebApplication1.Models;
using WebApplication1.Interface;

namespace WebApplication1.Data
{
	/*
        BestellingRepository is de datalaag voor orders.

        Belangrijk voor de docentfeedback:
        - De klant-bestelapplicatie schrijft in dezelfde tabellen: Bestellingen en Bestelregels.
        - De back-office leest exact diezelfde tabellen, dus ordernummer, status, annulering en klacht komen door.
        - Extra back-office velden worden automatisch toegevoegd als ze nog ontbreken.
          Hierdoor blijft de klantapp werken, want die hoeft deze extra kolommen niet te vullen.
    */
	public class BestellingRepository : IBestellingRepository
	{
        private readonly DatabaseConnection _databaseConnection;

        public BestellingRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public List<Bestelling> GetAlleBestellingen()
        {
            var bestellingen = new List<Bestelling>();

            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();
            EnsureOrderWorkflowColumns(connection);

            using var command = new SqlCommand(@"
                SELECT b.Id, b.Ordernummer, b.KlantNaam, b.Email, b.Datum, b.Status, b.Prioriteit,
                       COALESCE(b.VerzendAdres, '') AS VerzendAdres,
                       COALESCE(b.OrderOpmerking, '') AS OrderOpmerking,
                       COALESCE(b.PakketStatus, 'Nog niet ingepakt') AS PakketStatus,
                       b.BezorgerId,
                       COALESCE(m.Naam, '') AS BezorgerNaam,
                       COUNT(br.Id) AS AantalRegels,
                       COALESCE(SUM(CASE WHEN br.IsIngepakt = 1 THEN 1 ELSE 0 END), 0) AS AantalIngepakteRegels,
                       COALESCE(SUM(br.Prijs * br.Aantal), 0) AS Totaal
                FROM Bestellingen b
                LEFT JOIN Bestelregels br ON b.Id = br.BestellingId
                LEFT JOIN Medewerkers m ON b.BezorgerId = m.Id
                GROUP BY b.Id, b.Ordernummer, b.KlantNaam, b.Email, b.Datum, b.Status, b.Prioriteit,
                         b.VerzendAdres, b.OrderOpmerking, b.PakketStatus, b.BezorgerId, m.Naam
                ORDER BY b.Datum DESC;", connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                bestellingen.Add(MapBestelling(reader));
            }

            return bestellingen;
        }

        public Bestelling? GetBestellingById(int id)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();
            EnsureOrderWorkflowColumns(connection);

            using var command = new SqlCommand(@"
                SELECT b.Id, b.Ordernummer, b.KlantNaam, b.Email, b.Datum, b.Status, b.Prioriteit,
                       COALESCE(b.VerzendAdres, '') AS VerzendAdres,
                       COALESCE(b.OrderOpmerking, '') AS OrderOpmerking,
                       COALESCE(b.PakketStatus, 'Nog niet ingepakt') AS PakketStatus,
                       b.BezorgerId,
                       COALESCE(m.Naam, '') AS BezorgerNaam,
                       COUNT(br.Id) AS AantalRegels,
                       COALESCE(SUM(CASE WHEN br.IsIngepakt = 1 THEN 1 ELSE 0 END), 0) AS AantalIngepakteRegels,
                       COALESCE(SUM(br.Prijs * br.Aantal), 0) AS Totaal
                FROM Bestellingen b
                LEFT JOIN Bestelregels br ON b.Id = br.BestellingId
                LEFT JOIN Medewerkers m ON b.BezorgerId = m.Id
                WHERE b.Id = @Id
                GROUP BY b.Id, b.Ordernummer, b.KlantNaam, b.Email, b.Datum, b.Status, b.Prioriteit,
                         b.VerzendAdres, b.OrderOpmerking, b.PakketStatus, b.BezorgerId, m.Naam;", connection);

            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            var bestelling = MapBestelling(reader);
            reader.Close();

            bestelling.Regels = GetBestelregelsVoorBestelling(connection, id);
            bestelling.AantalRegels = bestelling.Regels.Count;
            bestelling.AantalIngepakteRegels = bestelling.Regels.Count(r => r.IsIngepakt);
            return bestelling;
        }

        public List<Medewerker> GetBezorgers()
        {
            var bezorgers = new List<Medewerker>();

            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();

            using var command = new SqlCommand(@"
                SELECT Id, Naam, Rol, Email, LaatstActief
                FROM Medewerkers
                WHERE Rol LIKE '%Bezorger%' OR Rol LIKE '%Bezorging%'
                ORDER BY Naam;", connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                bezorgers.Add(new Medewerker
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Naam = reader.GetString(reader.GetOrdinal("Naam")),
                    Rol = reader.GetString(reader.GetOrdinal("Rol")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    LaatstActief = reader.IsDBNull(reader.GetOrdinal("LaatstActief"))
                        ? "Nog niet actief"
                        : reader.GetDateTime(reader.GetOrdinal("LaatstActief")).ToString("dd-MM-yyyy HH:mm")
                });
            }

            // Als de database nog geen bezorgers bevat, tonen we alle medewerkers zodat de dropdown niet leeg is.
            if (bezorgers.Any())
            {
                return bezorgers;
            }

            reader.Close();
            using var fallbackCommand = new SqlCommand(@"
                SELECT Id, Naam, Rol, Email, LaatstActief
                FROM Medewerkers
                ORDER BY Naam;", connection);

            using var fallbackReader = fallbackCommand.ExecuteReader();
            while (fallbackReader.Read())
            {
                bezorgers.Add(new Medewerker
                {
                    Id = fallbackReader.GetInt32(fallbackReader.GetOrdinal("Id")),
                    Naam = fallbackReader.GetString(fallbackReader.GetOrdinal("Naam")),
                    Rol = fallbackReader.GetString(fallbackReader.GetOrdinal("Rol")),
                    Email = fallbackReader.GetString(fallbackReader.GetOrdinal("Email")),
                    LaatstActief = fallbackReader.IsDBNull(fallbackReader.GetOrdinal("LaatstActief"))
                        ? "Nog niet actief"
                        : fallbackReader.GetDateTime(fallbackReader.GetOrdinal("LaatstActief")).ToString("dd-MM-yyyy HH:mm")
                });
            }

            return bezorgers;
        }

        public void UpdateBestelregelIngepakt(int regelId, bool isIngepakt)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();
            EnsureOrderWorkflowColumns(connection);

            using var command = new SqlCommand(@"
                UPDATE Bestelregels
                SET IsIngepakt = @IsIngepakt
                WHERE Id = @RegelId;", connection);

            command.Parameters.AddWithValue("@RegelId", regelId);
            command.Parameters.AddWithValue("@IsIngepakt", isIngepakt);
            command.ExecuteNonQuery();
        }

        public void UpdateInpaklijst(int bestellingId, List<int> ingepakteRegelIds)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();
            EnsureOrderWorkflowColumns(connection);

            using var transaction = connection.BeginTransaction();

            try
            {
                using var resetCommand = new SqlCommand(@"
                    UPDATE Bestelregels
                    SET IsIngepakt = 0
                    WHERE BestellingId = @BestellingId;", connection, transaction);

                resetCommand.Parameters.AddWithValue("@BestellingId", bestellingId);
                resetCommand.ExecuteNonQuery();

                if (ingepakteRegelIds.Any())
                {
                    var parameterNamen = ingepakteRegelIds
                        .Select((_, index) => $"@RegelId{index}")
                        .ToArray();

                    using var updateCommand = new SqlCommand($@"
                        UPDATE Bestelregels
                        SET IsIngepakt = 1
                        WHERE BestellingId = @BestellingId
                          AND Id IN ({string.Join(", ", parameterNamen)});", connection, transaction);

                    updateCommand.Parameters.AddWithValue("@BestellingId", bestellingId);

                    for (var i = 0; i < ingepakteRegelIds.Count; i++)
                    {
                        updateCommand.Parameters.AddWithValue(parameterNamen[i], ingepakteRegelIds[i]);
                    }

                    updateCommand.ExecuteNonQuery();
                }

                using var statusCommand = new SqlCommand(@"
                    UPDATE Bestellingen
                    SET Status = 'Ingepakt',
                        PakketStatus = 'Ingepakt'
                    WHERE Id = @BestellingId
                      AND EXISTS (
                          SELECT 1
                          FROM Bestelregels
                          WHERE BestellingId = @BestellingId
                      )
                      AND NOT EXISTS (
                          SELECT 1
                          FROM Bestelregels
                          WHERE BestellingId = @BestellingId
                            AND IsIngepakt = 0
                      );", connection, transaction);

                statusCommand.Parameters.AddWithValue("@BestellingId", bestellingId);
                statusCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateStatus(int bestellingId, string status)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();
            EnsureOrderWorkflowColumns(connection);

            if (StatusVereistAllesIngepakt(status) && !ZijnAlleRegelsIngepakt(connection, bestellingId))
            {
                return false;
            }

            var pakketStatus = BepaalPakketStatus(status);
            using var command = new SqlCommand(@"
                UPDATE Bestellingen
                SET Status = @Status,
                    PakketStatus = @PakketStatus
                WHERE Id = @Id;", connection);

            command.Parameters.AddWithValue("@Id", bestellingId);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@PakketStatus", pakketStatus);
            command.ExecuteNonQuery();
            return true;
        }

        public void UpdateBezorger(int bestellingId, int? bezorgerId)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();
            EnsureOrderWorkflowColumns(connection);

            using var command = new SqlCommand(@"
                UPDATE Bestellingen
                SET BezorgerId = @BezorgerId,
                    PakketStatus = CASE
                        WHEN @BezorgerId IS NULL THEN PakketStatus
                        WHEN Status IN ('Onderweg') THEN 'Bij bezorger'
                        ELSE PakketStatus
                    END
                WHERE Id = @Id;", connection);

            command.Parameters.AddWithValue("@Id", bestellingId);
            command.Parameters.AddWithValue("@BezorgerId", bezorgerId.HasValue ? (object)bezorgerId.Value : DBNull.Value);
            command.ExecuteNonQuery();
        }

        private static List<Bestelregel> GetBestelregelsVoorBestelling(SqlConnection connection, int bestellingId)
        {
            var regels = new List<Bestelregel>();

            using var command = new SqlCommand(@"
                SELECT br.Id, br.ProductId, br.ProductNaam, br.Artikelnummer, br.Prijs, br.Aantal,
                       br.IsIngepakt,
                       COALESCE(p.Omschrijving, '') AS Omschrijving,
                       COALESCE(p.Merk, '') AS Merk,
                       COALESCE(c.Naam, '') AS Categorie,
                       COALESCE(p.Icoon, 'package') AS Icoon,
                       COALESCE(p.Voorraad, 0) AS Voorraad
                FROM Bestelregels br
                LEFT JOIN Producten p ON br.ProductId = p.Id
                LEFT JOIN Categorieen c ON p.CategorieId = c.Id
                WHERE br.BestellingId = @BestellingId
                ORDER BY br.Id;", connection);

            command.Parameters.AddWithValue("@BestellingId", bestellingId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                regels.Add(new Bestelregel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    ProductNaam = reader.GetString(reader.GetOrdinal("ProductNaam")),
                    Artikelnummer = reader.GetString(reader.GetOrdinal("Artikelnummer")),
                    Prijs = reader.GetDecimal(reader.GetOrdinal("Prijs")),
                    Aantal = reader.GetInt32(reader.GetOrdinal("Aantal")),
                    IsIngepakt = reader.GetBoolean(reader.GetOrdinal("IsIngepakt")),
                    Omschrijving = reader.GetString(reader.GetOrdinal("Omschrijving")),
                    Merk = reader.GetString(reader.GetOrdinal("Merk")),
                    Categorie = reader.GetString(reader.GetOrdinal("Categorie")),
                    Icoon = reader.GetString(reader.GetOrdinal("Icoon")),
                    Voorraad = reader.GetInt32(reader.GetOrdinal("Voorraad"))
                });
            }

            return regels;
        }

        private static Bestelling MapBestelling(SqlDataReader reader)
        {
            return new Bestelling
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Ordernummer = reader.GetString(reader.GetOrdinal("Ordernummer")),
                KlantNaam = reader.GetString(reader.GetOrdinal("KlantNaam")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                DatumTijd = reader.GetDateTime(reader.GetOrdinal("Datum")),
                Datum = reader.GetDateTime(reader.GetOrdinal("Datum")).ToString("dd-MM-yyyy"),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Prioriteit = reader.GetString(reader.GetOrdinal("Prioriteit")),
                VerzendAdres = reader.GetString(reader.GetOrdinal("VerzendAdres")),
                OrderOpmerking = reader.GetString(reader.GetOrdinal("OrderOpmerking")),
                PakketStatus = reader.GetString(reader.GetOrdinal("PakketStatus")),
                BezorgerId = reader.IsDBNull(reader.GetOrdinal("BezorgerId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("BezorgerId")),
                BezorgerNaam = reader.GetString(reader.GetOrdinal("BezorgerNaam")),
                AantalRegels = reader.GetInt32(reader.GetOrdinal("AantalRegels")),
                AantalIngepakteRegels = reader.GetInt32(reader.GetOrdinal("AantalIngepakteRegels")),
                Totaal = reader.GetDecimal(reader.GetOrdinal("Totaal"))
            };
        }

        private static bool ZijnAlleRegelsIngepakt(SqlConnection connection, int bestellingId)
        {
            using var command = new SqlCommand(@"
                SELECT COUNT(*)
                FROM Bestelregels
                WHERE BestellingId = @BestellingId
                  AND IsIngepakt = 0;", connection);

            command.Parameters.AddWithValue("@BestellingId", bestellingId);
            var openRegels = Convert.ToInt32(command.ExecuteScalar());
            return openRegels == 0;
        }

        private static bool StatusVereistAllesIngepakt(string status)
        {
            return status is "Ingepakt" or "Onderweg" or "Bezorgd";
        }

        private static string BepaalPakketStatus(string status)
        {
            return status switch
            {
                "Nieuw" => "Nog niet ingepakt",
                "In behandeling" => "Wordt verwerkt",
                "Ingepakt" => "Ingepakt",
                "Onderweg" => "Bij bezorger",
                "Bezorgd" => "Bezorgd",
                "Annulering aangevraagd" => "Wacht op annulering",
                "Geannuleerd" => "Geannuleerd",
                "Klacht gemeld" => "Klacht opvolgen",
                _ => status
            };
        }

        private static void EnsureOrderWorkflowColumns(SqlConnection connection)
        {
            using var command = new SqlCommand(@"
                IF COL_LENGTH('Bestellingen', 'VerzendAdres') IS NULL
                    ALTER TABLE Bestellingen ADD VerzendAdres NVARCHAR(250) NULL;

                IF COL_LENGTH('Bestellingen', 'OrderOpmerking') IS NULL
                    ALTER TABLE Bestellingen ADD OrderOpmerking NVARCHAR(500) NULL;

                IF COL_LENGTH('Bestellingen', 'PakketStatus') IS NULL
                    ALTER TABLE Bestellingen ADD PakketStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Bestellingen_PakketStatus DEFAULT 'Nog niet ingepakt';

                IF COL_LENGTH('Bestellingen', 'BezorgerId') IS NULL
                    ALTER TABLE Bestellingen ADD BezorgerId INT NULL;

                IF COL_LENGTH('Bestelregels', 'IsIngepakt') IS NULL
                    ALTER TABLE Bestelregels ADD IsIngepakt BIT NOT NULL CONSTRAINT DF_Bestelregels_IsIngepakt DEFAULT 0;", connection);

            command.ExecuteNonQuery();
        }
    }
}
