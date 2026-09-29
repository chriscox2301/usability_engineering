// JUNIOR-UITLEG: Bestand: KE03_INTDEV_SE_1_Base/Pages/Index.cshtml.cs
// Dit is de C#-code achter de homepagina Index.cshtml.
// Relatie:
// - De homepagina heeft geen database nodig.
// - De homepagina stuurt de klant met knoppen naar Producten/Overzicht.

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class StartModel : PageModel
    {
        // OnGet wordt uitgevoerd wanneer de homepagina opent.
        public void OnGet()
        {
            // Geen C# actie nodig.
            // Alle zichtbare inhoud staat in Index.cshtml en de styling in Start/start.css.
        }
    }
}
