using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication1.Infrastructure
{
    /*
        DecimalModelBinder zet tekst uit een formulier om naar een decimal (bijvoorbeeld Product.Prijs).

        Waarom een eigen binder?
        - De standaard binder gebruikt de cultuur van de server. Dan werkt "12,50" óf "12.50", maar niet allebei.
        - Deze binder accepteert allebei en zet de waarde om naar het juiste formaat.

        Regels:
        - "12,50" en "12.50" worden allebei 12.50.
        - Staan er zowel een punt als een komma in ("1.234,56" of "1,234.56"),
          dan is het laatste teken het decimaalteken en wordt het andere teken genegeerd.
        - Spaties en het euroteken worden weggehaald.
    */
    public class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            // SetModelValue zorgt dat de ingevulde tekst terugkomt in het formulier als er een fout is.
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

            var tekst = valueResult.FirstValue;
            var veldNaam = bindingContext.ModelMetadata.GetDisplayName();

            if (string.IsNullOrWhiteSpace(tekst))
            {
                // Bij decimal? is leeg toegestaan, bij decimal niet.
                if (Nullable.GetUnderlyingType(bindingContext.ModelType) != null)
                {
                    bindingContext.Result = ModelBindingResult.Success(null);
                }
                else
                {
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"Vul een waarde in voor {veldNaam}.");
                }
                return Task.CompletedTask;
            }

            if (TryParseBedrag(tekst, out var bedrag))
            {
                bindingContext.Result = ModelBindingResult.Success(bedrag);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                    $"'{tekst}' is geen geldig getal voor {veldNaam}. Gebruik bijvoorbeeld 12,50 of 12.50.");
            }

            return Task.CompletedTask;
        }

        // Public static zodat dezelfde omzetting ook elders gebruikt kan worden.
        public static bool TryParseBedrag(string tekst, out decimal bedrag)
        {
            var schoon = tekst.Replace("€", string.Empty).Replace(" ", string.Empty).Trim();

            var laatsteKomma = schoon.LastIndexOf(',');
            var laatstePunt = schoon.LastIndexOf('.');

            if (laatsteKomma >= 0 && laatstePunt >= 0)
            {
                // Het teken dat het laatst voorkomt is het decimaalteken, het andere is een duizendtalscheiding.
                schoon = laatsteKomma > laatstePunt
                    ? schoon.Replace(".", string.Empty).Replace(',', '.')
                    : schoon.Replace(",", string.Empty);
            }
            else
            {
                schoon = schoon.Replace(',', '.');
            }

            // InvariantCulture gebruikt altijd een punt als decimaalteken, ongeacht de instellingen van de server.
            return decimal.TryParse(schoon, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out bedrag);
        }
    }

    /*
        DecimalModelBinderProvider vertelt ASP.NET Core wanneer DecimalModelBinder gebruikt moet worden:
        voor elke property of parameter van het type decimal of decimal?.
    */
    public class DecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            var type = context.Metadata.ModelType;
            return type == typeof(decimal) || type == typeof(decimal?) ? new DecimalModelBinder() : null;
        }
    }
}
