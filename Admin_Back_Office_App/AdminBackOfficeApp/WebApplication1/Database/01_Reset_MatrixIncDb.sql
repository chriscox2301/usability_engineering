/*
    JUNIOR-UITLEG DATABASESCRIPT
    Relatie met C# code:
    - ProductRepository gebruikt de Producten, Categorieen en Leveranciers tabellen.
    - BestellingRepository gebruikt de Bestellingen en Bestelregels tabellen.
    - CategorieRepository, LeverancierRepository en MedewerkerRepository lezen hun eigen tabellen.
    - Stored procedures met sp_Producten_* worden aangeroepen vanuit ProductRepository.
*/


USE master;
GO

IF DB_ID('MatrixIncDb') IS NOT NULL
BEGIN
    DECLARE @killCommand NVARCHAR(MAX) = N'';

    SELECT @killCommand = @killCommand + N'KILL ' + CAST(session_id AS NVARCHAR(10)) + N';'
    FROM sys.dm_exec_sessions
    WHERE database_id = DB_ID('MatrixIncDb');

    IF LEN(@killCommand) > 0
    BEGIN
        EXEC sp_executesql @killCommand;
    END
END
GO

IF DB_ID('MatrixIncDb') IS NOT NULL
BEGIN
    ALTER DATABASE MatrixIncDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MatrixIncDb;
END
GO

CREATE DATABASE MatrixIncDb;
GO

USE MatrixIncDb;
GO

CREATE TABLE Categorieen
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL UNIQUE,
    Omschrijving NVARCHAR(500) NOT NULL,
    Icoon NVARCHAR(50) NOT NULL DEFAULT 'folder'
);
GO

CREATE TABLE Leveranciers
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL UNIQUE,
    Contactpersoon NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    Telefoon NVARCHAR(50) NOT NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Actief'
);
GO

CREATE TABLE Producten
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL,
    Omschrijving NVARCHAR(500) NOT NULL,
    Artikelnummer NVARCHAR(50) NOT NULL UNIQUE,
    Merk NVARCHAR(100) NOT NULL,
    CategorieId INT NOT NULL,
    LeverancierId INT NULL,
    Prijs DECIMAL(10,2) NOT NULL,
    Voorraad INT NOT NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Actief',
    Icoon NVARCHAR(50) NOT NULL DEFAULT 'package',
    LaatstGewijzigd DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Producten_Categorieen FOREIGN KEY (CategorieId) REFERENCES Categorieen(Id),
    CONSTRAINT FK_Producten_Leveranciers FOREIGN KEY (LeverancierId) REFERENCES Leveranciers(Id),
    CONSTRAINT CK_Producten_Prijs CHECK (Prijs >= 0),
    CONSTRAINT CK_Producten_Voorraad CHECK (Voorraad >= 0)
);
GO

CREATE TABLE Medewerkers
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    LaatstActief DATETIME2 NULL
);
GO

CREATE TABLE Bestellingen
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Ordernummer NVARCHAR(50) NOT NULL UNIQUE,
    KlantNaam NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    Datum DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Nieuw',
    Prioriteit NVARCHAR(30) NOT NULL DEFAULT 'Normaal',
    VerzendAdres NVARCHAR(250) NULL,
    OrderOpmerking NVARCHAR(500) NULL,
    PakketStatus NVARCHAR(50) NOT NULL DEFAULT 'Nog niet ingepakt',
    BezorgerId INT NULL,
    CONSTRAINT FK_Bestellingen_Bezorger FOREIGN KEY (BezorgerId) REFERENCES Medewerkers(Id)
);
GO

CREATE TABLE Bestelregels
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BestellingId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductNaam NVARCHAR(100) NOT NULL,
    Artikelnummer NVARCHAR(50) NOT NULL,
    Prijs DECIMAL(10,2) NOT NULL,
    Aantal INT NOT NULL,
    IsIngepakt BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Bestelregels_Bestellingen FOREIGN KEY (BestellingId) REFERENCES Bestellingen(Id),
    CONSTRAINT FK_Bestelregels_Producten FOREIGN KEY (ProductId) REFERENCES Producten(Id),
    CONSTRAINT CK_Bestelregels_Aantal CHECK (Aantal > 0)
);
GO

INSERT INTO Categorieen (Naam, Omschrijving, Icoon) VALUES
('Lagers', 'Onderdelen voor draaiende constructies.', 'circle-dot'),
('Aandrijving', 'Tandwielen, snaren en koppelingen.', 'cog'),
('Bevestiging', 'Bouten, moeren, ringen en klemmen.', 'bolt'),
('Hydrauliek', 'Koppelingen, leidingen en cilinders.', 'wrench'),
('Meetgereedschap', 'Meetmiddelen voor controle en kwaliteit.', 'gauge');
GO

INSERT INTO Leveranciers (Naam, Contactpersoon, Email, Telefoon, Status) VALUES
('Nordic Bearings', 'S. Jansen', 'inkoop@nordic.example', '043-1112222', 'Actief'),
('GearSupply BV', 'M. Peeters', 'sales@gearsupply.example', '043-3334444', 'Actief'),
('Fabory', 'K. Janssen', 'orders@fabory.example', '043-7778888', 'Actief'),
('HydroParts', 'A. Smeets', 'info@hydroparts.example', '043-5556666', 'Actief'),
('DriveTech', 'L. Hermans', 'sales@drivetech.example', '043-9990000', 'Actief'),
('MeasurePro', 'P. Willems', 'support@measurepro.example', '043-1212121', 'Actief');
GO

INSERT INTO Producten (Naam, Omschrijving, Artikelnummer, Merk, CategorieId, LeverancierId, Prijs, Voorraad, Status, Icoon) VALUES
('Kogellager 6204', 'Universeel lager voor roterende onderdelen.', 'MI-LGR-6204', 'SKF', 1, 1, 12.95, 4, 'Actief', 'circle-dot'),
('Tandwiel Module 2', 'Stalen tandwiel voor aandrijfsystemen.', 'MI-TDW-200', 'Bosch Rexroth', 2, 2, 39.50, 18, 'Actief', 'cog'),
('RVS Boutenset M8', 'Set met bouten, moeren en ringen.', 'MI-BVT-M8', 'Fabory', 3, 3, 8.75, 42, 'Actief', 'bolt'),
('Hydraulische koppeling', 'Koppeling voor hydraulische leidingen.', 'MI-HYD-K12', 'Parker', 4, 4, 24.20, 2, 'Actief', 'wrench'),
('V-snaar A37', 'Rubberen V-snaar voor machines.', 'MI-VSN-A37', 'Continental', 2, 5, 16.80, 27, 'Actief', 'rotate-cw'),
('Veerdrukmeter', 'Meetinstrument voor veerbelasting.', 'MI-MET-VDR', 'Mitutoyo', 5, 6, 119.00, 7, 'Actief', 'gauge');
GO

INSERT INTO Medewerkers (Naam, Rol, Email, LaatstActief) VALUES
('Back-office beheerder', 'Productbeheer', 'beheer@matrix.example', GETDATE()),
('Voorraadmedewerker', 'Voorraad', 'voorraad@matrix.example', DATEADD(day, -1, GETDATE())),
('Sam Bezorger', 'Bezorger', 'sam.bezorger@matrix.example', GETDATE()),
('Nora Bezorger', 'Bezorger', 'nora.bezorger@matrix.example', DATEADD(hour, -3, GETDATE()));
GO

INSERT INTO Bestellingen (Ordernummer, KlantNaam, Email, Datum, Status, Prioriteit, VerzendAdres, OrderOpmerking, PakketStatus, BezorgerId) VALUES
('ORD-2026-1001', 'TechnoParts Limburg', 'inkoop@technoparts.example', DATEADD(day, -1, GETDATE()), 'Nieuw', 'Hoog', 'Boschstraat 24, Maastricht', 'Graag leveren bij goederenontvangst.', 'Nog niet ingepakt', NULL),
('ORD-2026-1000', 'Machinebouw Zuid', 'info@machinebouwzuid.example', DATEADD(day, -2, GETDATE()), 'In behandeling', 'Normaal', 'Industrieweg 8, Sittard', '', 'Wordt verwerkt', NULL),
('ORD-2026-0999', 'Maintenance Group', 'orders@maintenance.example', DATEADD(day, -5, GETDATE()), 'Onderweg', 'Normaal', 'Stationsstraat 12, Heerlen', 'Afleveren tussen 09:00 en 12:00.', 'Bij bezorger', 3);
GO

INSERT INTO Bestelregels (BestellingId, ProductId, ProductNaam, Artikelnummer, Prijs, Aantal) VALUES
(1, 1, 'Kogellager 6204', 'MI-LGR-6204', 12.95, 4),
(1, 4, 'Hydraulische koppeling', 'MI-HYD-K12', 24.20, 2),
(2, 2, 'Tandwiel Module 2', 'MI-TDW-200', 39.50, 1),
(2, 3, 'RVS Boutenset M8', 'MI-BVT-M8', 8.75, 4),
(3, 5, 'V-snaar A37', 'MI-VSN-A37', 16.80, 6),
(3, 6, 'Veerdrukmeter', 'MI-MET-VDR', 119.00, 1);
GO

CREATE OR ALTER PROCEDURE sp_Producten_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Id, p.Naam, p.Omschrijving, p.Artikelnummer, p.Merk,
           c.Naam AS Categorie, p.Prijs, p.Voorraad, p.Status, p.Icoon,
           COALESCE(l.Naam, '') AS Leverancier, p.LaatstGewijzigd
    FROM Producten p
    INNER JOIN Categorieen c ON p.CategorieId = c.Id
    LEFT JOIN Leveranciers l ON p.LeverancierId = l.Id
    ORDER BY p.Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Producten_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Id, p.Naam, p.Omschrijving, p.Artikelnummer, p.Merk,
           c.Naam AS Categorie, p.Prijs, p.Voorraad, p.Status, p.Icoon,
           COALESCE(l.Naam, '') AS Leverancier, p.LaatstGewijzigd
    FROM Producten p
    INNER JOIN Categorieen c ON p.CategorieId = c.Id
    LEFT JOIN Leveranciers l ON p.LeverancierId = l.Id
    WHERE p.Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Producten_Insert
    @Naam NVARCHAR(100),
    @Omschrijving NVARCHAR(500),
    @Artikelnummer NVARCHAR(50),
    @Merk NVARCHAR(100),
    @Categorie NVARCHAR(100),
    @Prijs DECIMAL(10,2),
    @Voorraad INT,
    @Status NVARCHAR(30),
    @Icoon NVARCHAR(50),
    @Leverancier NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CategorieId INT = (SELECT Id FROM Categorieen WHERE Naam = @Categorie);
    IF @CategorieId IS NULL
    BEGIN
        INSERT INTO Categorieen (Naam, Omschrijving, Icoon) VALUES (@Categorie, 'Nieuwe categorie', 'folder');
        SET @CategorieId = SCOPE_IDENTITY();
    END

    DECLARE @LeverancierId INT = NULL;
    IF NULLIF(@Leverancier, '') IS NOT NULL
    BEGIN
        SELECT @LeverancierId = Id FROM Leveranciers WHERE Naam = @Leverancier;
        IF @LeverancierId IS NULL
        BEGIN
            INSERT INTO Leveranciers (Naam, Contactpersoon, Email, Telefoon, Status)
            VALUES (@Leverancier, 'Onbekend', 'onbekend@example.com', '-', 'Actief');
            SET @LeverancierId = SCOPE_IDENTITY();
        END
    END

    INSERT INTO Producten (Naam, Omschrijving, Artikelnummer, Merk, CategorieId, LeverancierId, Prijs, Voorraad, Status, Icoon, LaatstGewijzigd)
    VALUES (@Naam, @Omschrijving, @Artikelnummer, @Merk, @CategorieId, @LeverancierId, @Prijs, @Voorraad, @Status, @Icoon, GETDATE());
END
GO

CREATE OR ALTER PROCEDURE sp_Producten_Update
    @Id INT,
    @Naam NVARCHAR(100),
    @Omschrijving NVARCHAR(500),
    @Artikelnummer NVARCHAR(50),
    @Merk NVARCHAR(100),
    @Categorie NVARCHAR(100),
    @Prijs DECIMAL(10,2),
    @Voorraad INT,
    @Status NVARCHAR(30),
    @Icoon NVARCHAR(50),
    @Leverancier NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CategorieId INT = (SELECT Id FROM Categorieen WHERE Naam = @Categorie);
    IF @CategorieId IS NULL
    BEGIN
        INSERT INTO Categorieen (Naam, Omschrijving, Icoon) VALUES (@Categorie, 'Nieuwe categorie', 'folder');
        SET @CategorieId = SCOPE_IDENTITY();
    END

    DECLARE @LeverancierId INT = NULL;
    IF NULLIF(@Leverancier, '') IS NOT NULL
    BEGIN
        SELECT @LeverancierId = Id FROM Leveranciers WHERE Naam = @Leverancier;
        IF @LeverancierId IS NULL
        BEGIN
            INSERT INTO Leveranciers (Naam, Contactpersoon, Email, Telefoon, Status)
            VALUES (@Leverancier, 'Onbekend', 'onbekend@example.com', '-', 'Actief');
            SET @LeverancierId = SCOPE_IDENTITY();
        END
    END

    UPDATE Producten
    SET Naam = @Naam,
        Omschrijving = @Omschrijving,
        Artikelnummer = @Artikelnummer,
        Merk = @Merk,
        CategorieId = @CategorieId,
        LeverancierId = @LeverancierId,
        Prijs = @Prijs,
        Voorraad = @Voorraad,
        Status = @Status,
        Icoon = @Icoon,
        LaatstGewijzigd = GETDATE()
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Producten_Deactivate
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Producten
    SET Status = 'Inactief', LaatstGewijzigd = GETDATE()
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Producten_UpdateVoorraad
    @Id INT,
    @Voorraad INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Producten
    SET Voorraad = @Voorraad, LaatstGewijzigd = GETDATE()
    WHERE Id = @Id;
END
GO
