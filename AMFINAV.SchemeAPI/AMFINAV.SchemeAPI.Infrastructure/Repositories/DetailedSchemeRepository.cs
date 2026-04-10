using Microsoft.EntityFrameworkCore;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using AMFINAV.SchemeAPI.Infrastructure.Data;

namespace AMFINAV.SchemeAPI.Infrastructure.Repositories
{
    public class DetailedSchemeRepository : IDetailedSchemeRepository
    {
        private readonly ApplicationDbContext _context;

        public DetailedSchemeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsBySchemeCodeAndDateAsync(
            string schemeCode, DateTime navDate) =>
            await _context.DetailedSchemes
                .AnyAsync(d => d.SchemeCode == schemeCode
                            && d.NavDate == navDate.Date);

        public async Task AddRangeAsync(IEnumerable<DetailedScheme> schemes) =>
            await _context.DetailedSchemes.AddRangeAsync(schemes);

        // ← new — bulk update IsApproved for all schemes under a fund
        public async Task UpdateApprovalByFundCodeAsync(string fundCode, bool isApproved)
        {
            var schemes = await _context.DetailedSchemes
                .Where(d => d.FundCode == fundCode)
                .ToListAsync();

            foreach (var scheme in schemes)
                scheme.IsApproved = isApproved;

            _context.DetailedSchemes.UpdateRange(schemes);
        }
    }
}