// JUNIOR-UITLEG: Bestand: KE03_INTDEV_SE_1_Base/Pages/Account/Inloggen.cshtml.cs
// Dit is de C#-code achter de inlogpagina.
// Relatie:
// - De pagina is toegevoegd voor een realistische webshopuitstraling.
// - De inlogfunctie hoeft niet te werken voor de opdracht.
// - De knop "Doorgaan als gast" verwijst naar de productpagina.

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages.Account
{
    public class InloggenModel : PageModel
    {
        public string ReturnUrl { get; set; } = "/Producten/Overzicht";

        public void OnGet(string? returnUrl)
        {
            // Geen echte loginlogica. De knop stuurt de gebruiker door naar de juiste vervolgpagina.
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/Producten/Overzicht" : returnUrl;
        }
    }
}
