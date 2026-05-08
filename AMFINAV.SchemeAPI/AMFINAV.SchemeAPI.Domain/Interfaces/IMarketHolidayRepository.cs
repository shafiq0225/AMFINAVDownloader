using AMFINAV.SchemeAPI.Domain.Entities;

namespace AMFINAV.SchemeAPI.Domain.Interfaces
{
    public interface IMarketHolidayRepository
    {
        Task<bool> ExistsByDateAsync(DateTime date);
        Task AddAsync(MarketHoliday holiday);
        Task<MarketHoliday?> GetByDateAsync(DateTime date);
    }
}