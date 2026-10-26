using Microsoft.Data.SqlClient;
using WebApplication1.Data;
using WebApplication1.Infrastructure;

namespace WebApplication1
{
    /*
        Program.cs is het startpunt van de ASP.NET Core MVC-applicatie.

        Technische leesroute voor junior developers:
        1. WebApplication.CreateBuilder(args) maakt een builder aan die services en configuratie verzamelt.
        2. builder.Services.AddControllersWithViews() zet MVC aan. Daardoor werken Controllers en Views.
        3. AddScoped<T>() registreert repositories voor Dependency Injection.
           - Scoped betekent: één instantie per HTTP-request.
           - Controllers krijgen deze objecten via hun constructor.
        4. builder.Build() maakt de echte webapplicatie.
        5. Middleware zoals UseStaticFiles(), UseRouting() en UseAuthorization() bepaalt hoe requests verwerkt worden.
        6. MapControllerRoute() bepaalt welke controller/action standaard geopend wordt.
    */
    public class Program
    {
        public static void Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("Applicatie gestart. ");



            // Builder bevat configuratie uit appsettings.json, environment settings en geregistreerde services.
            var builder = WebApplication.CreateBuilder(args);

            // appsettings.Local.json staat niet in git. Daarin zet elke developer zijn eigen SQL Server-naam.
            // Dit bestand wordt als laatste geladen, dus het overschrijft appsettings.json.
            builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

            // Registreert MVC. Zonder deze regel werken controllers zoals DashboardController niet.
            builder.Services.AddControllersWithViews(options =>
            {
                // Eigen binder voor decimal: accepteert zowel een komma als een punt (12,50 of 12.50).
                // Insert(0, ...) zorgt dat deze binder vóór de standaard binder van ASP.NET Core komt.
                options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());

                // Standaard foutmeldingen van model binding zijn Engels. Hier vervangen we ze door Nederlandse teksten.
                var berichten = options.ModelBindingMessageProvider;
                berichten.SetAttemptedValueIsInvalidAccessor((waarde, veld) => $"'{waarde}' is geen geldige waarde voor {veld}.");
                berichten.SetMissingBindRequiredValueAccessor(veld => $"Er is geen waarde opgegeven voor {veld}.");
                berichten.SetMissingKeyOrValueAccessor(() => "Er is een waarde verplicht.");
                berichten.SetMissingRequestBodyRequiredValueAccessor(() => "Er is een niet-lege request body verplicht.");
                berichten.SetNonPropertyAttemptedValueIsInvalidAccessor(waarde => $"'{waarde}' is geen geldige waarde.");
                berichten.SetNonPropertyUnknownValueIsInvalidAccessor(() => "De opgegeven waarde is ongeldig.");
                berichten.SetNonPropertyValueMustBeANumberAccessor(() => "Dit veld moet een getal zijn.");
                berichten.SetUnknownValueIsInvalidAccessor(veld => $"De opgegeven waarde is ongeldig voor {veld}.");
                berichten.SetValueIsInvalidAccessor(waarde => $"'{waarde}' is geen geldige waarde.");
                berichten.SetValueMustBeANumberAccessor(veld => $"{veld} moet een getal zijn.");
                berichten.SetValueMustNotBeNullAccessor(_ => "Dit veld is verplicht.");
            });

            // DatabaseConnection leest de connection string en maakt SqlConnection-objecten.
            builder.Services.AddScoped<DatabaseConnection>();

            // Repositories vormen de datalaag. Controllers gebruiken deze klassen in plaats van direct SQL te schrijven.
            builder.Services.AddScoped<ProductRepository>();
            builder.Services.AddScoped<BestellingRepository>();
            builder.Services.AddScoped<CategorieRepository>();
            builder.Services.AddScoped<LeverancierRepository>();
            builder.Services.AddScoped<MedewerkerRepository>();
            builder.Services.AddScoped<KlantenserviceRepository>();

            // Bouwt de applicatie op basis van alle instellingen hierboven.
            var app = builder.Build();


            // Integratietest bij het opstarten: werkt de verbinding met de database?
            // Het resultaat komt in de console, direct onder "Applicatie gestart.".
            using (var scope = app.Services.CreateScope())
            {
                var databaseConnection = scope.ServiceProvider.GetRequiredService<DatabaseConnection>();

                ControleerDatabaseConnectie(databaseConnection, logger);
            }


            // In productie wordt een nette foutpagina gebruikt in plaats van technische exceptiondetails.
            if (!app.Environment.IsDevelopment())
            {
                // Bij een fout routeert ASP.NET Core naar HomeController.Error().
                app.UseExceptionHandler("/Home/Error");

                // HSTS dwingt browsers om HTTPS te gebruiken nadat de site eenmaal via HTTPS is bezocht.
                app.UseHsts();
            }

            // Stuurt HTTP-verzoeken automatisch door naar HTTPS.
            app.UseHttpsRedirection();

            // Maakt bestanden in wwwroot beschikbaar, zoals CSS, JavaScript, Bootstrap en afbeeldingen.
            app.UseStaticFiles();

            // Activeert routeherkenning. Daarna weet ASP.NET Core welke controller/action bij een URL hoort.
            app.UseRouting();

            // Activeert autorisatie-middleware. Er is nu geen login, maar deze regel is normaal in MVC-projecten.
            app.UseAuthorization();

            // Standaardroute:
            // /Producten/Details/5 betekent controller=Producten, action=Details, id=5.
            // Zonder URL opent de app DashboardController.Index().
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}");

            // Start de webserver.
            app.Run();
        }


        /*
            Opent een verbinding met de database, voert "SELECT 1" uit en logt of dat gelukt is.
            Geeft true terug als de database werkt, anders false.
            Public static, zodat de xUnit-test in TestProject dezelfde controle kan aanroepen.
        */
        public static bool ControleerDatabaseConnectie(DatabaseConnection databaseConnection, ILogger logger)
        {
            try
            {
                using var verbinding = databaseConnection.MaakVerbinding();

                verbinding.Open();

                using var command = new SqlCommand("SELECT 1", verbinding);

                command.ExecuteScalar();

                logger.LogInformation("Databaseconnectie werkt: verbonden met database {Database} op server {Server}.", verbinding.Database, verbinding.DataSource);

                return true;
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "Databaseconnectie werkt niet: {Reden}", ex.Message);

                return false;
            }
        }
    }
    
}
