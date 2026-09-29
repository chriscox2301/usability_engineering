/*
    site.js

    Technische uitleg:
    - _Layout.cshtml laadt dit bestand op iedere pagina.
    - DOMContentLoaded wacht tot de HTML volledig geladen is voordat JavaScript elementen zoekt.
    - Lucide iconen worden vervangen op basis van <i data-lucide="..."></i>.
    - Het hamburgermenu werkt door een CSS-class op <body> aan/uit te zetten.
*/

// Eerste DOMContentLoaded-blok: activeert Lucide iconen.
document.addEventListener('DOMContentLoaded', function () {
    // window.lucide bestaat alleen als het externe Lucide script goed geladen is.
    if (window.lucide) {
        // createIcons zoekt alle data-lucide attributen en vervangt ze door SVG-iconen.
        window.lucide.createIcons();
    }
});

// Tweede DOMContentLoaded-blok: regelt het mobiele sidebar-menu.
document.addEventListener('DOMContentLoaded', function () {
    // Knop in _Layout.cshtml waarmee de gebruiker het menu opent.
    const menuButton = document.getElementById('mobileMenuButton');

    // Donkere/doorzichtige laag achter het menu. Klikken hierop sluit het menu.
    const backdrop = document.getElementById('sidebarBackdrop');

    // Alle navigatielinks in de sidebar. Na klikken sluiten we het menu op mobiel.
    const sidebarLinks = document.querySelectorAll('.admin-sidebar .side-nav a');

    // Centrale helperfunctie: opent of sluit de sidebar.
    function setMenuOpen(isOpen) {
        // CSS in site.css gebruikt body.sidebar-open om de sidebar zichtbaar te maken.
        document.body.classList.toggle('sidebar-open', isOpen);

        // aria-expanded helpt browsers en hulpmiddelen begrijpen of het menu open staat.
        if (menuButton) {
            menuButton.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        }
    }

    // Klik op hamburgermenu: wissel tussen open en dicht.
    if (menuButton) {
        menuButton.addEventListener('click', function () {
            setMenuOpen(!document.body.classList.contains('sidebar-open'));
        });
    }

    // Klik op backdrop: sluit het menu.
    if (backdrop) {
        backdrop.addEventListener('click', function () {
            setMenuOpen(false);
        });
    }

    // Klik op een navigatielink: sluit het menu, handig op kleine schermen.
    sidebarLinks.forEach(function (link) {
        link.addEventListener('click', function () {
            setMenuOpen(false);
        });
    });
});
