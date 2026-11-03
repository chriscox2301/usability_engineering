/*
    producten/index.js

    Technische uitleg:
    - Views/Producten/Index.cshtml laadt dit bestand via @section Scripts.
    - Elke <tr class="product-row-link"> heeft een data-href met de URL van ProductenController.Details(id).
    - Een <tr> kan zelf geen link zijn, daarom vangen we de klik hier op en navigeren we naar die URL.
    - Klikken op een echte link (Details, Wijzigen, Verwijderen) laten we gewoon door,
      zodat die knoppen naar hun eigen pagina blijven gaan.
*/
document.addEventListener('DOMContentLoaded', function () {
    const rijen = document.querySelectorAll('tr.product-row-link[data-href]');

    rijen.forEach(function (rij) {
        rij.addEventListener('click', function (event) {
            // Klik op een actieknop: niets doen, de link zelf handelt de navigatie af.
            if (event.target.closest('a, button')) {
                return;
            }

            // Ctrl/Cmd-klik of middelste muisknop opent de details in een nieuw tabblad, net als bij een gewone link.
            if (event.ctrlKey || event.metaKey) {
                window.open(rij.dataset.href, '_blank');
                return;
            }

            window.location.href = rij.dataset.href;
        });

        rij.addEventListener('auxclick', function (event) {
            if (event.button === 1 && !event.target.closest('a, button')) {
                window.open(rij.dataset.href, '_blank');
            }
        });
    });
});
