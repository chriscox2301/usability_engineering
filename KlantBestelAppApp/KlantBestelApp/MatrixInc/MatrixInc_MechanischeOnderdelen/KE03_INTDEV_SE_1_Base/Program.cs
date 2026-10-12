using DataAccessLayer;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Repositories;

namespace KE03_INTDEV_SE_1_Base
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // appsettings.Local.json staat niet in git: eigen SQL Server-naam per developer.
            builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

            // Zelfde database-aanpak als de MVC back-office applicatie:
            // DatabaseConnection leest ConnectionStrings:DefaultConnection uit appsettings.json
            // en repositories gebruiken ADO.NET met Microsoft.Data.SqlClient.
            builder.Services.AddScoped<DatabaseConnection>();

            builder.Services.AddScoped<IBestellingRepository, BestellingRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddRazorPages();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();
            app.MapRazorPages();
            app.Run();
        }
    }
}
