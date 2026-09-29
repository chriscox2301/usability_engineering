using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer.Repositories
{
    /*
        BestellingRepository gebruikt dezelfde MatrixIncDb-tabellen als de MVC back-office:
        - Bestellingen
        - Bestelregels
        - Producten
        - Medewerkers

        Belangrijk:
        - Als de klant bestelt, komt exact dezelfde order in de back-office binnen.
        - Hetzelfde ordernummer, dezelfde bestelregels en dezelfde status worden gebruikt.
        - De voorraad in Producten wordt direct verlaagd tijdens het plaatsen van de bestelling.
        - Annulering/klacht vanuit de klantapp wijzigt dezelfde Status-kolom die de back-office leest.
    */
    public class BestellingRepository : IBestellingRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public BestellingRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public List<GeplaatsteBestelling> HaalOrderhistorieOp(string? email = null)
        {
            var bestellingen = HaalBestellingenUitDatabase(email);
            return bestellingen
                .Select(bestelling => MapNaarGeplaatsteBestelling(bestelling))
                .OrderByDescending(bestelling => bestelling.Besteldatum)
                .ToList();
        }

        public GeplaatsteBestelling? HaalOrderhistorieItemOp(string ordernummer)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();
            EnsureOrderWorkflowColumns(connection);

            using var command = new SqlCommand(@"
                SELECT Id
                FROM Bestellingen
                WHERE Ordernummer = @Ordernummer;", connection);
            command.Parameters.AddWithValue("@Ordernummer", ordernummer);

            var idObject = command.ExecuteScalar();
            if (idObject == null)
            {
                return null;
            }

            var bestelling = HaalBestellingHeader(connection, Convert.ToInt32(idObject));
            if (bestelling == null)
            {
                return null;
            }

            bestelling.Regels = HaalBestelregels(connection, bestelling.Id);
            return MapNaarGeplaatsteBestelling(bestelling);
        }

        public string PlaatsBestelling(GeplaatsteBestelling bestelling)
        {
            if (string.IsNullOrWhiteSpace(bestelling.Ordernummer))
            {
                bestelling.Ordernummer = $"ORD-{DateTime.Now:yyyyMMdd-HHmmss}";
            }

            if (bestelling.Besteldatum == default)
            {
                bestelling.Besteldatum = DateTime.Now;
            }

            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();
            EnsureOrderWorkflowColumns(connection);
            using var transaction = connection.BeginTransaction();

            try
            {
                ControleerEnReserveerVoorraad(connection, transaction, bestelling.Regels);

                int bestellingId;
                using (var command = new SqlCommand(@"
                    INSERT INTO Bestellingen
                        (Ordernummer, KlantNaam, Email, Datum, Status, Prioriteit, VerzendAdres, OrderOpmerking, PakketStatus, BezorgerId)
                    VALUES
                        (@Ordernummer, @KlantNaam, @Email, @Datum, @Status, @Prioriteit, @VerzendAdres, @OrderOpmerking, @PakketStatus, NULL);
                    SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
                {
                    command.Parameters.AddWithValue("@Ordernummer", bestelling.Ordernummer);
                    command.Parameters.AddWithValue("@KlantNaam", string.IsNullOrWhiteSpace(bestelling.KlantNaam) ? "Onbekende klant" : bestelling.KlantNaam);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(bestelling.Email) ? "onbekend@example.com" : bestelling.Email);
                    command.Parameters.AddWithValue("@Datum", bestelling.Besteldatum);
                    command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(bestelling.Status) ? "Nieuw" : bestelling.Status);
                    command.Parameters.AddWithValue("@Prioriteit", "Normaal");
                    command.Parameters.AddWithValue("@VerzendAdres", string.IsNullOrWhiteSpace(bestelling.VerzendAdres) ? DBNull.Value : bestelling.VerzendAdres);
                    command.Parameters.AddWithValue("@OrderOpmerking", string.IsNullOrWhiteSpace(bestelling.OrderOpmerkingen) ? DBNull.Value : bestelling.OrderOpmerkingen);
                    command.Parameters.AddWithValue("@PakketStatus", "Nog niet ingepakt");
                    bestellingId = Convert.ToInt32(command.ExecuteScalar());
                }

                foreach (var regel in bestelling.Regels)
                {
                    using var regelCommand = new SqlCommand(@"
                        INSERT INTO Bestelregels
                            (BestellingId, ProductId, ProductNaam, Artikelnummer, Prijs, Aantal, IsIngepakt)
                        VALUES
                            (@BestellingId, @ProductId, @ProductNaam, @Artikelnummer, @Prijs, @Aantal, 0);", connection, transaction);

                    regelCommand.Parameters.AddWithValue("@BestellingId", bestellingId);
                    regelCommand.Parameters.AddWithValue("@ProductId", regel.ProductId);
                    regelCommand.Parameters.AddWithValue("@ProductNaam", regel.ProductNaam);
                    regelCommand.Parameters.AddWithValue("@Artikelnummer", string.IsNullOrWhiteSpace(regel.Artikelnummer) ? $"P-{regel.ProductId}" : regel.Artikelnummer);
                    regelCommand.Parameters.AddWithValue("@Prijs", regel.Prijs);
                    regelCommand.Parameters.AddWithValue("@Aantal", regel.Aantal);
                    regelCommand.ExecuteNonQuery();
                }

                transaction.Commit();
                return bestelling.Ordernummer;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void VraagAnnuleringAan(string ordernummer)
        {
            UpdateStatus(ordernummer, "Annulering aangevraagd", "Wacht op annulering");
        }

        public void RegistreerKlachtmelding(string ordernummer)
        {
            UpdateStatus(ordernummer, "Klacht gemeld", "Klacht opvolgen");
        }

        private void UpdateStatus(string ordernummer, string status, string pakketStatus)
        {
            using var connection = _databaseConnection.MaakVerbinding();
            connection.Open();
            EnsureOrderWorkflowColumns(connection);

            using var command = new SqlCommand(@"
                UPDATE Bestellingen
                SET Status = @Status,
                    PakketStatus = @PakketStatus
                WHERE Ordernummer = @Ordernummer;", connection);

            command.Parameters.AddWithValue("@Ordernummer", ordernummer);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@PakketStatus", pakketStatus);
            command.ExecuteNonQuery();
        }

        private static void ControleerEnReserveerVoorraad(SqlConnection connection, SqlTransaction transaction, List<Bestelregel> regels)
        {
            foreach (var regel in regels)
            {
                using var voorraadCommand = new SqlCommand(@"
                    SELECT Voorraad, Status
                    FROM Producten WITH (UPDLOCK, HOLDLOCK)
                    WHERE Id = @ProductId;", connection, transaction);
                voorraadCommand.Parameters.AddWithValue("@ProductId", regel.ProductId);

                using var reader = voorraadCommand.ExecuteReader();
                if (!reader.Read())
                {
                    throw new InvalidOperationException($"Product met id {regel.ProductId} bestaat niet meer.");
                }

                var huidigeVoorraad = reader.GetInt32(reader.GetOrdinal("Voorraad"));
                var status = reader.GetString(reader.GetOrdinal("Status"));
                reader.Close();

                if (!status.Equals("Actief", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"{regel.ProductNaam} is niet actief en kan niet besteld worden.");
                }

                if (huidigeVoorraad < regel.Aantal)
                {
                    throw new InvalidOperationException($"Er is onvoldoende voorraad voor {regel.ProductNaam}. Beschikbaar: {huidigeVoorraad}, gevraagd: {regel.Aantal}.");
                }

                using var updateCommand = new SqlCommand(@"
                    UPDATE Producten
                    SET Voorraad = Voorraad - @Aantal,
                        LaatstGewijzigd = GETDATE()
                    WHERE Id = @ProductId;", connection, transaction);
                updateCommand.Parameters.AddWithValue("@Aantal", regel.Aantal);
                updateCommand.Parameters.AddWithValue("@ProductId", regel.ProductId);
                updateCommand.ExecuteNonQuery();
            }
        }

        private List<Bestelling> HaalBestellingenUitDatabase(string? email = null)
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
                       COALESCE(SUM(br.Prijs * br.Aantal), 0) AS Totaal
                FROM Bestellingen b
                LEFT JOIN Bestelregels br ON b.Id = br.BestellingId
                LEFT JOIN Medewerkers m ON b.BezorgerId = m.Id
                WHERE (@Email IS NULL OR b.Email = @Email)
                GROUP BY b.Id, b.Ordernummer, b.KlantNaam, b.Email, b.Datum, b.Status, b.Prioriteit,
                         b.VerzendAdres, b.OrderOpmerking, b.PakketStatus, b.BezorgerId, m.Naam
                ORDER BY b.Datum DESC;", connection);

            command.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? DBNull.Value : email);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                bestellingen.Add(MapBestelling(reader));
            }

            reader.Close();
            foreach (var bestelling in bestellingen)
            {
                bestelling.Regels = HaalBestelregels(connection, bestelling.Id);
            }

            return bestellingen;
        }

        private static Bestelling? HaalBestellingHeader(SqlConnection connection, int id)
        {
            using var command = new SqlCommand(@"
                SELECT b.Id, b.Ordernummer, b.KlantNaam, b.Email, b.Datum, b.Status, b.Prioriteit,
                       COALESCE(b.VerzendAdres, '') AS VerzendAdres,
                       COALESCE(b.OrderOpmerking, '') AS OrderOpmerking,
                       COALESCE(b.PakketStatus, 'Nog niet ingepakt') AS PakketStatus,
                       b.BezorgerId,
                       COALESCE(m.Naam, '') AS BezorgerNaam,
                       COUNT(br.Id) AS AantalRegels,
                       COALESCE(SUM(br.Prijs * br.Aantal), 0) AS Totaal
                FROM Bestellingen b
                LEFT JOIN Bestelregels br ON b.Id = br.BestellingId
                LEFT JOIN Medewerkers m ON b.BezorgerId = m.Id
                WHERE b.Id = @Id
                GROUP BY b.Id, b.Ordernummer, b.KlantNaam, b.Email, b.Datum, b.Status, b.Prioriteit,
                         b.VerzendAdres, b.OrderOpmerking, b.PakketStatus, b.BezorgerId, m.Naam;", connection);

            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapBestelling(reader) : null;
        }

        private static List<Bestelregel> HaalBestelregels(SqlConnection connection, int bestellingId)
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
            var datum = reader.GetDateTime(reader.GetOrdinal("Datum"));
            return new Bestelling
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Ordernummer = reader.GetString(reader.GetOrdinal("Ordernummer")),
                KlantNaam = reader.GetString(reader.GetOrdinal("KlantNaam")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                DatumTijd = datum,
                Datum = datum.ToString("dd-MM-yyyy"),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Prioriteit = reader.GetString(reader.GetOrdinal("Prioriteit")),
                VerzendAdres = reader.GetString(reader.GetOrdinal("VerzendAdres")),
                OrderOpmerking = reader.GetString(reader.GetOrdinal("OrderOpmerking")),
                PakketStatus = reader.GetString(reader.GetOrdinal("PakketStatus")),
                BezorgerId = reader.IsDBNull(reader.GetOrdinal("BezorgerId")) ? null : reader.GetInt32(reader.GetOrdinal("BezorgerId")),
                BezorgerNaam = reader.GetString(reader.GetOrdinal("BezorgerNaam")),
                AantalRegels = reader.GetInt32(reader.GetOrdinal("AantalRegels")),
                Totaal = reader.GetDecimal(reader.GetOrdinal("Totaal"))
            };
        }

        private static GeplaatsteBestelling MapNaarGeplaatsteBestelling(Bestelling bestelling)
        {
            return new GeplaatsteBestelling
            {
                Id = bestelling.Id,
                Ordernummer = bestelling.Ordernummer,
                Besteldatum = bestelling.DatumTijd,
                Status = bestelling.Status,
                Totaalbedrag = bestelling.Totaal,
                KlantNaam = bestelling.KlantNaam,
                Email = bestelling.Email,
                VerzendAdres = bestelling.VerzendAdres,
                OrderOpmerkingen = bestelling.OrderOpmerking,
                PakketStatus = bestelling.PakketStatus,
                BezorgerNaam = bestelling.BezorgerNaam,
                BezorgMelding = MaakBezorgMelding(bestelling.Status, bestelling.PakketStatus, bestelling.BezorgerNaam),
                AnnuleringAangevraagd = bestelling.Status.Equals("Annulering aangevraagd", StringComparison.OrdinalIgnoreCase),
                KlachtAangemaakt = bestelling.Status.Equals("Klacht gemeld", StringComparison.OrdinalIgnoreCase),
                Regels = bestelling.Regels
            };
        }

        private static string MaakBezorgMelding(string status, string pakketStatus, string bezorgerNaam)
        {
            return status switch
            {
                "Nieuw" => "Je bestelling is ontvangen en zichtbaar voor de back-office.",
                "In behandeling" => "De back-office verwerkt je bestelling.",
                "Ingepakt" => "Je bestelling is ingepakt.",
                "Verzonden" => "Je bestelling is meegegeven aan de bezorging.",
                "Onderweg" => string.IsNullOrWhiteSpace(bezorgerNaam) ? "Je bestelling is onderweg." : $"Je bestelling staat in de lijst van {bezorgerNaam}.",
                "Bezorgd" => "Je bestelling is bezorgd.",
                "Annulering aangevraagd" => "Annulering is aangevraagd. Back-office moet dit beoordelen.",
                "Geannuleerd" => "Je bestelling is geannuleerd.",
                "Klacht gemeld" => "Klacht is gemeld. Klantenservice neemt contact op.",
                _ => string.IsNullOrWhiteSpace(pakketStatus) ? "Geen extra bezorgprobleem bekend." : $"Pakketstatus: {pakketStatus}."
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
                    ALTER TABLE Bestellingen ADD PakketStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Bestellingen_PakketStatus_Klant DEFAULT 'Nog niet ingepakt';

                IF COL_LENGTH('Bestellingen', 'BezorgerId') IS NULL
                    ALTER TABLE Bestellingen ADD BezorgerId INT NULL;

                IF COL_LENGTH('Bestelregels', 'IsIngepakt') IS NULL
                    ALTER TABLE Bestelregels ADD IsIngepakt BIT NOT NULL CONSTRAINT DF_Bestelregels_IsIngepakt_Klant DEFAULT 0;", connection);

            command.ExecuteNonQuery();
        }
    }
}
