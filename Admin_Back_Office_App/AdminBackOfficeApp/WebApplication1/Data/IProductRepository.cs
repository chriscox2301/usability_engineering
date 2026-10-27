using WebApplication1.Models;

namespace WebApplication1.Data
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