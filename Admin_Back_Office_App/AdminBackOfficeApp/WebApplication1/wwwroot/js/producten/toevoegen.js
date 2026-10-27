/*
    producten/toevoegen.js

    Technische uitleg:
    - Views/Producten/Toevoegen.cshtml laadt dit bestand via @section Scripts.
    - Dit script controleert Prijs, Voorraad, Artikelnummer en de verplichte velden
      (Naam, Merk, Categorie, Omschrijving) direct tijdens het invullen,
      zodat de gebruiker niet pas na "Opslaan" een melding krijgt.
    - De server controleert alles nog een keer (Product.cs + DecimalModelBinder.cs).
      Deze controle is dus alleen voor gebruiksgemak.
*/
document.addEventListener('DOMContentLoaded', function () {
    const formulier = document.querySelector('form.form-card');
    const prijsInput = document.getElementById('Prijs');
    const voorraadInput = document.getElementById('Voorraad');
    const artikelnummerInput = document.getElementById('Artikelnummer');

    if (!formulier || !prijsInput || !voorraadInput || !artikelnummerInput) {
        return;
    }

    // Toont of verbergt een melding in de <span asp-validation-for="..."> onder het veld.
    function toonMelding(input, melding) {
        const span = document.querySelector('[data-valmsg-for="' + input.name + '"]');
        if (span) {
            span.textContent = melding;
        }
        input.classList.toggle('is-invalid', melding !== '');
        // Na een mislukte POST zet de server "input-validation-error" op het veld. Die halen we weg zodra het klopt.
        if (melding === '') {
            input.classList.remove('input-validation-error');
        }
        return melding === '';
    }

    // Verplichte velden zonder extra regels: alleen controleren of er iets is ingevuld/gekozen.
    // De meldingen zijn dezelfde als de [Required]-meldingen in Product.cs.
    const verplichteVelden = [
        { input: document.getElementById('Naam'), melding: 'Vul een naam in.' },
        { input: document.getElementById('Merk'), melding: 'Vul een merk in.' },
        { input: document.getElementById('Categorie'), melding: 'Kies een categorie.' },
        { input: document.getElementById('Omschrijving'), melding: 'Vul een omschrijving in.' }
    ].filter(function (veld) { return veld.input !== null; });

    function controleerVerplicht(veld) {
        return toonMelding(veld.input, veld.input.value.trim() === '' ? veld.melding : '');
    }

    // Zet "12,5", "12.5" of "1.234,56" om naar een getal. Geeft null terug als het geen geldig bedrag is.
    // Dezelfde regels als DecimalModelBinder.TryParseBedrag() op de server.
    function leesBedrag(tekst) {
        let schoon = tekst.replace(/€/g, '').replace(/\s/g, '');
        const laatsteKomma = schoon.lastIndexOf(',');
        const laatstePunt = schoon.lastIndexOf('.');

        if (laatsteKomma >= 0 && laatstePunt >= 0) {
            schoon = laatsteKomma > laatstePunt
                ? schoon.replace(/\./g, '').replace(',', '.')
                : schoon.replace(/,/g, '');
        } else {
            schoon = schoon.replace(',', '.');
        }

        return /^-?\d+(\.\d+)?$/.test(schoon) ? parseFloat(schoon) : null;
    }

    function controleerPrijs() {
        const tekst = prijsInput.value.trim();
        if (tekst === '') {
            return toonMelding(prijsInput, 'Vul een prijs in.');
        }

        const bedrag = leesBedrag(tekst);
        if (bedrag === null) {
            return toonMelding(prijsInput, 'Vul een geldige prijs in, bijvoorbeeld 12,50 of 12.50.');
        }
        if (bedrag <= 0) {
            return toonMelding(prijsInput, 'Prijs moet groter zijn dan 0.');
        }
        return toonMelding(prijsInput, '');
    }

    // Na het verlaten van het veld zetten we de prijs om naar het juiste formaat: twee decimalen met een komma.
    function formatteerPrijs() {
        const bedrag = leesBedrag(prijsInput.value.trim());
        if (bedrag !== null && bedrag > 0) {
            prijsInput.value = bedrag.toFixed(2).replace('.', ',');
        }
    }

    function controleerVoorraad() {
        const tekst = voorraadInput.value.trim();
        if (tekst === '') {
            return toonMelding(voorraadInput, 'Vul een voorraad in.');
        }
        if (/^-\d+$/.test(tekst)) {
            return toonMelding(voorraadInput, 'Voorraad kan niet negatief zijn.');
        }
        if (!/^\d+$/.test(tekst)) {
            return toonMelding(voorraadInput, 'Voorraad moet een heel getal zijn, bijvoorbeeld 10.');
        }
        return toonMelding(voorraadInput, '');
    }

    function controleerArtikelnummer() {
        const tekst = artikelnummerInput.value.trim();
        if (tekst === '') {
            return toonMelding(artikelnummerInput, 'Vul een artikelnummer in.');
        }
        if (!/\d/.test(tekst)) {
            return toonMelding(artikelnummerInput, 'Artikelnummer moet minimaal 1 cijfer bevatten.');
        }
        return toonMelding(artikelnummerInput, '');
    }

    // Een nieuw product heeft prijs 0. We maken het veld leeg zodat de placeholder "Bijv. 12,50" zichtbaar is.
    if (leesBedrag(prijsInput.value.trim()) === 0) {
        prijsInput.value = '';
    }

    // 'input' gaat af bij elke toetsaanslag, dus de melding verschijnt meteen.
    voorraadInput.addEventListener('input', controleerVoorraad);

    // Prijs en artikelnummer: een lege of halve waarde tijdens het typen is normaal.
    // Daarom controleren we pas na het verlaten van het veld (blur), en daarna live zodra er al een fout staat.
    prijsInput.addEventListener('blur', function () {
        if (controleerPrijs()) {
            formatteerPrijs();
        }
    });
    prijsInput.addEventListener('input', function () {
        if (prijsInput.classList.contains('is-invalid')) {
            controleerPrijs();
        }
    });

    artikelnummerInput.addEventListener('blur', controleerArtikelnummer);
    artikelnummerInput.addEventListener('input', function () {
        if (artikelnummerInput.classList.contains('is-invalid')) {
            controleerArtikelnummer();
        }
    });

    // Verplichte velden: na het verlaten (of kiezen bij een dropdown) controleren, daarna live zodra er een fout staat.
    verplichteVelden.forEach(function (veld) {
        veld.input.addEventListener('blur', function () { controleerVerplicht(veld); });
        veld.input.addEventListener('change', function () { controleerVerplicht(veld); });
        veld.input.addEventListener('input', function () {
            if (veld.input.classList.contains('is-invalid')) {
                controleerVerplicht(veld);
            }
        });
    });

    // Bij opslaan alles controleren, zodat de gebruiker in één keer álle ontbrekende velden ziet.
    // Bij een fout wordt het formulier niet verstuurd.
    formulier.addEventListener('submit', function (event) {
        // map() (geen every()) zodat elk veld gecontroleerd wordt, ook als een eerder veld al fout is.
        const verplichtOk = verplichteVelden.map(controleerVerplicht).every(Boolean);
        const prijsOk = controleerPrijs();
        const voorraadOk = controleerVoorraad();
        const artikelnummerOk = controleerArtikelnummer();

        if (!verplichtOk || !prijsOk || !voorraadOk || !artikelnummerOk) {
            event.preventDefault();
            const eersteFout = formulier.querySelector('.is-invalid');
            if (eersteFout) {
                eersteFout.focus();
            }
        }
    });
});
