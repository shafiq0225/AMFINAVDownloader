using AMFINAV.Investment.Domain.Entities;

namespace AMFINAV.Investment.Domain.Interfaces
{
    public interface IHoldingRepository
    {
        // ── Create ────────────────────────────────────────────────
        Task<Holding> AddAsync(Holding holding);

        // ── Read ──────────────────────────────────────────────────
        Task<Holding?> GetByIdAsync(int id);
        Task<Holding?> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Holding>> GetAllActiveAsync();
        Task<IEnumerable<Holding>> GetByInvestorAsync(string investorUserId);

        // ── Update ────────────────────────────────────────────────
        Task UpdateAsync(Holding holding);

        // ── Helpers ───────────────────────────────────────────────
        Task<bool> ExistsForOrderAsync(int orderId);

        Task<IEnumerable<Holding>> GetAllByInvestorActiveAsync(string investorUserId);
        Task<IEnumerable<Holding>> GetAllActiveGroupedAsync();

    }
}