using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces
{
    /*
        Interface voor bestellingen in de klant-bestelapp.
        De klantapp gebruikt alleen order plaatsen, orderhistorie tonen en klantacties.
        Orderbeheer en inpakflow zitten in de admin back-office app.
    */
    public interface IBestellingRepository
    {
        List<GeplaatsteBestelling> HaalOrderhistorieOp(string? email = null);
        GeplaatsteBestelling? HaalOrderhistorieItemOp(string ordernummer);
        string PlaatsBestelling(GeplaatsteBestelling bestelling);
        void VraagAnnuleringAan(string ordernummer);
        void RegistreerKlachtmelding(string ordernummer);
    }
}
