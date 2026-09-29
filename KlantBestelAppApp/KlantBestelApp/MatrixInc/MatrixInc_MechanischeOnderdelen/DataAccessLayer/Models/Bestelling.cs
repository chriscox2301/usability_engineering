namespace DataAccessLayer.Models
{
    /*
        Bestelling-model in lijn met de MVC back-office database.
        Dit model hoort bij SQL Server tabel Bestellingen.
    */
    public class Bestelling
    {
        public int Id { get; set; }
        public string Ordernummer { get; set; } = string.Empty;
        public string KlantNaam { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DatumTijd { get; set; }
        public string Datum { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Prioriteit { get; set; } = "Normaal";
        public decimal Totaal { get; set; }
        public int AantalRegels { get; set; }
        public string VerzendAdres { get; set; } = string.Empty;
        public string OrderOpmerking { get; set; } = string.Empty;
        public string PakketStatus { get; set; } = "Nog niet ingepakt";
        public int? BezorgerId { get; set; }
        public string BezorgerNaam { get; set; } = string.Empty;
        public List<Bestelregel> Regels { get; set; } = new();
    }
}
