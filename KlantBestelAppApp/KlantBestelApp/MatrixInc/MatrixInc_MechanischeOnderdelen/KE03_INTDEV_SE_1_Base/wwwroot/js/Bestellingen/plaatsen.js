
document.addEventListener("DOMContentLoaded", function () {
    const formulier = document.querySelector(".checkout-form");

    if (!formulier) {
        return;
    }

    const naam = document.getElementById("Naam");
    const email = document.getElementById("Email");
    const adres = document.getElementById("Adres");
    const postcode = document.getElementById("Postcode");
    const plaats = document.getElementById("Plaats");

    function toonMelding(input, melding) {
        const span = document.querySelector(
            '[data-valmsg-for="' + input.name + '"]'
        );

        if (span) {
            span.textContent = melding;
        }

        input.classList.toggle("is-invalid", melding !== "");
        input.classList.remove("input-validation-error");

        return melding === "";
    }

    function controleerVerplicht(input, naamVeld) {
        if (input.value.trim() === "") {
            return toonMelding(input, "Vul " + naamVeld + " in.");
        }

        return toonMelding(input, "");
    }

    function controleerEmail() {
        const waarde = email.value.trim();

        if (waarde === "") {
            return toonMelding(email, "Vul je e-mailadres in.");
        }

        const emailPatroon = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!emailPatroon.test(waarde)) {
            return toonMelding(
                email,
                "Vul een geldig e-mailadres in, bijvoorbeeld naam@example.com."
            );
        }

        return toonMelding(email, "");
    }

    function controleerPostcode() {
        const waarde = postcode.value.trim();

        if (waarde === "") {
            return toonMelding(postcode, "Vul je postcode in.");
        }

        const postcodePatroon = /^[1-9][0-9]{3}\s?[A-Za-z]{2}$/;

        if (!postcodePatroon.test(waarde)) {
            return toonMelding(
                postcode,
                "Vul een geldige Nederlandse postcode in, bijvoorbeeld 1234 AB."
            );
        }

        return toonMelding(postcode, "");
    }

    // Controleer een veld wanneer de gebruiker het verlaat.
    naam.addEventListener("blur", function () {
        controleerVerplicht(naam, "je naam");
    });

    email.addEventListener("blur", controleerEmail);
    adres.addEventListener("blur", function () {
        controleerVerplicht(adres, "je adres");
    });

    postcode.addEventListener("blur", controleerPostcode);

    plaats.addEventListener("blur", function () {
        controleerVerplicht(plaats, "je woonplaats");
    });

    // Wanneer een fout is gevonden, controleer opnieuw tijdens het typen.
    [naam, adres, plaats].forEach(function (input) {
        input.addEventListener("input", function () {
            if (input.classList.contains("is-invalid")) {
                controleerVerplicht(input, 
                    input === naam ? "je naam" :
                    input === adres ? "je adres" : "je woonplaats"
                );
            }
        });
    });

    email.addEventListener("input", function () {
        if (email.classList.contains("is-invalid")) {
            controleerEmail();
        }
    });

    postcode.addEventListener("input", function () {
        if (postcode.classList.contains("is-invalid")) {
            controleerPostcode();
        }
    });

    // Controleer alle velden voordat het formulier wordt verstuurd.
    formulier.addEventListener("submit", function (event) {
        const naamOk = controleerVerplicht(naam, "je naam");
        const emailOk = controleerEmail();
        const adresOk = controleerVerplicht(adres, "je adres");
        const postcodeOk = controleerPostcode();
        const plaatsOk = controleerVerplicht(plaats, "je woonplaats");

        if (!naamOk || !emailOk || !adresOk ||
            !postcodeOk || !plaatsOk) {

            event.preventDefault();

            const eersteFout =
                formulier.querySelector(".is-invalid");

            if (eersteFout) {
                eersteFout.focus();
            }
        }
    });
});
