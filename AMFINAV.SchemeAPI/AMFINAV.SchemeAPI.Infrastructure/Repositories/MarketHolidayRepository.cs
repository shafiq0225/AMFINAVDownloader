using Microsoft.EntityFrameworkCore;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using AMFINAV.SchemeAPI.Infrastructure.Data;

namespace AMFINAV.SchemeAPI.Infrastructure.Repositories
{
    public class MarketHolidayRepository : IMarketHolidayRepository
    {
        private readonly ApplicationDbContext _context;

        public MarketHolidayRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByDateAsync(DateTime date) =>
            await _context.MarketHolidays
                .AnyAsync(h => h.HolidayDate == date.Date);

        public async Task AddAsync(MarketHoliday holiday) =>
            await _context.MarketHolidays.AddAsync(holiday);

        public async Task<MarketHoliday?> GetByDateAsync(DateTime date) =>
            await _context.MarketHolidays
                .FirstOrDefaultAsync(h => h.HolidayDate == date.Date);
    }
}