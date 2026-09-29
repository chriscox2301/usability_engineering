namespace WebApplication1.Models
{
    /*
        ViewModel: AdminDashboardViewModel

        Een ViewModel is een model dat speciaal voor één scherm is samengesteld.
        Dit model bevat dashboardtellingen én detailoverzichten.
        DashboardController vult dit object en Views/Dashboard/Index.cshtml leest het via @model.
    */
    public class AdminDashboardViewModel
    {
        // Getal voor KPI-kaart: producten met lage voorraad.
        public int AantalLageVoorraadProducten { get; set; }

        // Getal voor KPI-kaart: bestellingen met status Nieuw.
        public int AantalNieuweBestellingen { get; set; }

        // Getal voor KPI-kaart: openstaande klantenservice-klachten.
        public int AantalOpenstaandeKlachten { get; set; }

        // Getal voor KPI-kaart: open bestellingen met prioriteit Hoog.
        public int AantalHogePrioriteitBestellingen { get; set; }

        // Lijst voor het detailblok Voorraad met risico.
        public List<Product> ProductenMetLageVoorraad { get; set; } = new();

        // Lijst voor het detailblok Klantenservice.
        public List<Klacht> OpenstaandeKlachten { get; set; } = new();

        // Lijst voor het detailblok Nieuwe bestellingen.
        public List<Bestelling> TeVerwerkenBestellingen { get; set; } = new();

        // Lijst voor het detailblok Hoge prioriteit.
        public List<Bestelling> HogePrioriteitBestellingen { get; set; } = new();
    }
}
