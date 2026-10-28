using WebApplication1.Models;

namespace WebApplication1.Interface
{
    public interface IBestellingRepository
    {
        List<Bestelling> GetAlleBestellingen();
        Bestelling? GetBestellingById(int id);
        List<Medewerker> GetBezorgers();
        void UpdateBestelregelIngepakt(int regelId, bool isIngepakt);
        void UpdateInpaklijst(int bestellingId, List<int> ingepakteRegelIds);
        bool UpdateStatus(int bestellingId, string status);
        void UpdateBezorger(int bestellingId, int? bezorgerId);
    }
}
