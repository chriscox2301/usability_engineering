# Matrix Inc. Back-office

ASP.NET Core MVC-applicatie voor het beheren van producten, voorraad, bestellingen, categorieën, leveranciers, rapportages en medewerkers.

## Database

Voer eerst het script uit in SSMS:

```text
Database/01_Reset_MatrixIncDb.sql
```

Controleer daarna de connection string in `appsettings.json`.

## Belangrijke onderdelen

- `Data/DatabaseConnection.cs`
- `Data/ProductRepository.cs`
- `Data/BestellingRepository.cs`
- `Data/CategorieRepository.cs`
- `Data/LeverancierRepository.cs`
- `Data/MedewerkerRepository.cs`

## Startpagina

De applicatie opent standaard op:

```text
/Dashboard/Index
```
