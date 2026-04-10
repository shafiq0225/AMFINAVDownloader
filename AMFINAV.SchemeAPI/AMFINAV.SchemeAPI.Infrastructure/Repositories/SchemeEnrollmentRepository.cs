using Microsoft.EntityFrameworkCore;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using AMFINAV.SchemeAPI.Infrastructure.Data;

namespace AMFINAV.SchemeAPI.Infrastructure.Repositories
{
    public class SchemeEnrollmentRepository : ISchemeEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public SchemeEnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SchemeEnrollment>> GetAllAsync() =>
            await _context.SchemeEnrollments
                .OrderBy(s => s.SchemeCode)
                .ToListAsync();

        public async Task<SchemeEnrollment?> GetBySchemeCodeAsync(string schemeCode) =>
            await _context.SchemeEnrollments
                .FirstOrDefaultAsync(s => s.SchemeCode == schemeCode);

        public async Task<IEnumerable<SchemeEnrollment>> GetApprovedSchemesAsync() =>
            await _context.SchemeEnrollments
                .Where(s => s.IsApproved)
                .OrderBy(s => s.SchemeCode)
                .ToListAsync();

        public async Task<IEnumerable<SchemeEnrollment>> GetByFundCodeAsync(string fundCode) =>
            await _context.SchemeEnrollments
                .Where(s => s.FundCode == fundCode)
                .OrderBy(s => s.SchemeCode)
                .ToListAsync();

        public async Task<bool> ExistsBySchemeCodeAsync(string schemeCode) =>
            await _context.SchemeEnrollments
                .AnyAsync(s => s.SchemeCode == schemeCode);

        public async Task AddAsync(SchemeEnrollment scheme) =>
            await _context.SchemeEnrollments.AddAsync(scheme);

        public async Task UpdateAsync(string schemeCode, SchemeEnrollment updated)
        {
            var existing = await _context.SchemeEnrollments
                .FirstOrDefaultAsync(s => s.SchemeCode == schemeCode);

            if (existing is null) return;

            existing.SchemeName = updated.SchemeName;
            existing.IsApproved = updated.IsApproved;
            existing.UpdatedAt = updated.UpdatedAt;

            _context.SchemeEnrollments.Update(existing);
        }

        // ← new — bulk update all schemes under a fund
        public async Task UpdateApprovalByFundCodeAsync(string fundCode, bool isApproved)
        {
            var schemes = await _context.SchemeEnrollments
                .Where(s => s.FundCode == fundCode)
                .ToListAsync();

            foreach (var scheme in schemes)
            {
                scheme.IsApproved = isApproved;
                scheme.UpdatedAt = DateTime.UtcNow;
            }

            _context.SchemeEnrollments.UpdateRange(schemes);
        }
    }
}