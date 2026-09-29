# Klant-bestelapp opgeschoond

Deze versie is opgeschoond zodat de klant-bestelapp alleen nog de logica bevat die echt gebruikt wordt.

## Verwijderd

- Oude `MatrixIncDbContext.cs`.
- Oude `MatrixIncDbInitializer.cs`.
- Oude klant-repository die niets deed.
- Oude onderdeel-repository die niets deed.
- Oude `Klant` en `Onderdeel` modellen uit de base-solution.
- Niet-gebruikte CRUD-methodes voor productbeheer in de klantapp.
- Niet-gebruikte CRUD-methodes voor orderbeheer in de klantapp.
- Oude navigatie-properties die alleen nog voor Entity Framework bedoeld waren.
- Oude documentatie die nog over tijdelijke orderhistorie in Session ging.

## Wat blijft bewust wel bestaan

- `Session` blijft bestaan voor de winkelmand.
- De winkelmand is tijdelijk per bezoeker en hoeft nog niet direct in SQL Server te staan.
- Producten, voorraad, bestellingen, bestelregels, annuleringen en klachten lopen via SQL Server `MatrixIncDb`.

## Actieve klantapp-flow

1. Producten worden gelezen uit SQL Server tabel `Producten`.
2. De klant stopt producten tijdelijk in de winkelmand via Session.
3. Bij bestelling plaatsen wordt de order opgeslagen in SQL Server:
   - `Bestellingen`
   - `Bestelregels`
4. De voorraad in `Producten` wordt verlaagd.
5. De orderhistorie wordt uit SQL Server gelezen.
6. Annulering en klacht wijzigen dezelfde orderstatus die de back-office gebruikt.

## Belangrijk

De klantapp gebruikt nog steeds dezelfde database als de admin back-office app:

```json
"DefaultConnection": "Server=localhost;Database=MatrixIncDb;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30;"
```
