using Microsoft.EntityFrameworkCore;
using AMFINAV.Domain.Entities;
using AMFINAV.Domain.Interfaces;
using AMFINAV.Infrastructure.Data;

namespace AMFINAV.Infrastructure.Repositories
{
    public class NavFileRepository : INavFileRepository
    {
        private readonly ApplicationDbContext _context;

        public NavFileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task<NavFile> GetByDateAsync(DateTime date)
        //{
        //    return await _context.NavFiles
        //        .FirstOrDefaultAsync(f => f.NavDate == date.Date);
        //}

        //public async Task<IEnumerable<DateTime>> GetAllDatesAsync()
        //{
        //    return await _context.NavFiles
        //        .Select(f => f.NavDate)
        //        .OrderByDescending(d => d)
        //        .ToListAsync();
        //}

        //public async Task<DateTime?> GetLatestDateAsync()
        //{
        //    return await _context.NavFiles
        //        .MaxAsync(f => (DateTime?)f.NavDate);
        //}

        public async Task<bool> ExistsByDateAsync(DateTime date)
        {
            return await _context.NavFiles
                .AnyAsync(f => f.NavDate == date.Date);
        }

        public async Task AddAsync(NavFile navFile)
        {
            await _context.NavFiles.AddAsync(navFile);
        }
    }
}