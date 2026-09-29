// JUNIOR-UITLEG: Bestand: DataAccessLayer/Models/SessieExtensions.cs
// Deze helperklasse maakt Session makkelijker te gebruiken.
// Session kan standaard alleen simpele tekst opslaan.
// Met deze functies zetten we objecten/lijsten om naar JSON-tekst en weer terug naar C# objecten.
// Relaties:
// - Winkelmand gebruikt Session met List<WinkelmandItem>.

using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace DataAccessLayer.Models
{
    public static class SessieExtensions
    {
        // ZetObject slaat een C# object op in Session.
        // T betekent: het werkt met elk type, bijvoorbeeld List<WinkelmandItem>.
        // sleutel is de naam waaronder we het bewaren, bijvoorbeeld "Winkelmand".
        public static void ZetObject<T>(this ISession sessie, string sleutel, T waarde)
        {
            // Serialize verandert het object naar JSON-tekst.
            sessie.SetString(sleutel, JsonSerializer.Serialize(waarde));
        }

        // HaalObject haalt een object uit Session.
        // Als er niets gevonden wordt, geeft deze functie null/default terug.
        public static T? HaalObject<T>(this ISession sessie, string sleutel)
        {
            // Eerst halen we de JSON-tekst op uit Session.
            var json = sessie.GetString(sleutel);

            // Als er geen JSON bestaat, is de winkelmand of orderhistorie nog leeg.
            // Anders wordt de JSON teruggezet naar een C# object met Deserialize.
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }
    }
}
