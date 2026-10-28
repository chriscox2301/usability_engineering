using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetAlleProducten();
        Product? GetProductById(int id);
        void VoegProductToe(Product product);
        void WijzigProduct(Product product);
        void DeactiveerProduct(int id);
        bool VerwijderConceptProduct(int id);
        void PasVoorraadAan(int id, int voorraad);
        List<string> GetCategorieNamen();
        List<string> GetLeverancierNamen();
    }
}