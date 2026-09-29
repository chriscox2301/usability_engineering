using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces
{
    /*
        Interface voor de klant-bestelapp.
        Alleen functies die de klantapp echt gebruikt staan hierin.
        Productbeheer gebeurt in de admin back-office app, niet in deze klantapp.
    */
    public interface IProductRepository
    {
        IEnumerable<Product> HaalAlleProductenOp();
        Product? HaalProductOpMetId(int id);
    }
}
