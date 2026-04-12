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

        // ← Get all SchemeCodes under a FundCode
        public async Task<IEnumerable<string>> GetSchemeCodesByFundCodeAsync(
            string fundCode) =>
            await _context.DetailedSchemes
                .Where(d => d.FundCode == fundCode)
                .Select(d => d.SchemeCode)
                .Distinct()
                .ToListAsync();

        // ← Bulk update IsApproved for all DetailedScheme rows under a FundCode
        public async Task UpdateApprovalByFundCodeAsync(
            string fundCode, bool isApproved)
        {
            var schemes = await _context.DetailedSchemes
                .Where(d => d.FundCode == fundCode)
                .ToListAsync();

            if (schemes.Count == 0) return;

            foreach (var scheme in schemes)
                scheme.IsApproved = isApproved;

            _context.DetailedSchemes.UpdateRange(schemes);
        }

        /// <summary>
        /// Fetches records in the date range PLUS the nearest previous record
        /// per scheme before startDate — needed to calculate the first entry's
        /// percentage correctly.
        /// </summary>
        public async Task<IEnumerable<DetailedScheme>> GetByDateRangeWithPreviousAsync(
            DateTime startDate, DateTime endDate)
        {
            // Step 1 — Records within requested range
            var inRange = await _context.DetailedSchemes
                .Where(d => d.IsApproved
                         && d.NavDate >= startDate.Date
                         && d.NavDate <= endDate.Date)
                .ToListAsync();

            // Step 2 — For each scheme, find the nearest record BEFORE startDate
            var schemeCodes = inRange.Select(d => d.SchemeCode).Distinct().ToList();

            var previousRecords = new List<DetailedScheme>();

            foreach (var schemeCode in schemeCodes)
            {
                var previous = await _context.DetailedSchemes
                    .Where(d => d.SchemeCode == schemeCode
                             && d.NavDate < startDate.Date)
                    .OrderByDescending(d => d.NavDate)
                    .FirstOrDefaultAsync();

                if (previous != null)
                    previousRecords.Add(previous);
            }

            // Step 3 — Combine and return ordered
            return inRange
                .Concat(previousRecords)
                .OrderBy(d => d.SchemeCode)
                .ThenBy(d => d.NavDate)
                .ToList();
        }

        // ← Returns last N distinct NavDates that have actual data
        public async Task<List<DateTime>> GetLastTradingDatesAsync(int count) =>
            await _context.DetailedSchemes
                .Where(d => d.IsApproved)
                .Select(d => d.NavDate)
                .Distinct()
                .OrderByDescending(d => d)
                .Take(count)
                .ToListAsync();
    }
}