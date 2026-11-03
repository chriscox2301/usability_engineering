/*
    icoon-voorbeeld.js

    Laat naast de icoon-dropdown een voorbeeld zien van het gekozen Lucide-icoon.
    Gebruikt in Producten/Toevoegen.cshtml en Producten/Wijzigen.cshtml.

    Waarom een nieuwe <i> maken?
    lucide.createIcons() vervangt <i data-lucide="..."> door een <svg>.
    Na de eerste keer staat er dus geen <i> meer die we kunnen aanpassen.
*/
(function () {
    // asp-for="Icoon" geeft de select automatisch id="Icoon".
    const icoonSelect = document.getElementById("Icoon");
    const voorbeeld = document.getElementById("icoon-voorbeeld");

    // Staat een van de twee niet op de pagina? Dan doen we niks.
    if (!icoonSelect || !voorbeeld) return;

    function werkVoorbeeldBij() {
        // Haal het oude icoon (de <svg>) weg.
        voorbeeld.innerHTML = "";

        // Maak een nieuwe <i data-lucide="..."> met het gekozen icoon.
        const icoon = document.createElement("i");
        icoon.setAttribute("data-lucide", icoonSelect.value || "package");
        voorbeeld.appendChild(icoon);

        // Lucide is van internet geladen (unpkg). Als dat mislukt is, slaan we dit over.
        if (window.lucide) {
            window.lucide.createIcons();
        }
    }

    // Bij elke andere keuze in de dropdown werken we het voorbeeld bij.
    icoonSelect.addEventListener("change", werkVoorbeeldBij);

    // Ook één keer bij het laden, zodat het opgeslagen of gekozen icoon er meteen staat.
    werkVoorbeeldBij();
})();