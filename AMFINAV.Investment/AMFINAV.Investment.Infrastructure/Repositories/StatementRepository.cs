using AMFINAV.Investment.Domain.Entities;
using AMFINAV.Investment.Domain.Interfaces;
using AMFINAV.Investment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMFINAV.Investment.Infrastructure.Repositories
{
    public class StatementRepository : IStatementRepository
    {
        private readonly InvestmentDbContext _context;

        public StatementRepository(InvestmentDbContext context)
        {
            _context = context;
        }

        // ── Create ────────────────────────────────────────────────
        public async Task<InvestmentStatement> AddAsync(
            InvestmentStatement statement)
        {
            await _context.InvestmentStatements.AddAsync(statement);
            return statement;
        }

        // ── Read ──────────────────────────────────────────────────
        public async Task<InvestmentStatement?> GetByIdAsync(int id)
        {
            return await _context.InvestmentStatements
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<InvestmentStatement?> GetByOrderIdAsync(int orderId)
        {
            return await _context.InvestmentStatements
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.OrderId == orderId);
        }

        public async Task<IEnumerable<InvestmentStatement>> GetByInvestorAsync(
            string investorUserId)
        {
            return await _context.InvestmentStatements
                .Include(s => s.Order)
                .Where(s => s.InvestorUserId == investorUserId)
                .OrderByDescending(s => s.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InvestmentStatement>> GetAllAsync()
        {
            return await _context.InvestmentStatements
                .Include(s => s.Order)
                .OrderByDescending(s => s.UploadedAt)
                .ToListAsync();
        }

        // ── Helpers ───────────────────────────────────────────────
        public async Task<bool> ExistsForOrderAsync(int orderId)
        {
            return await _context.InvestmentStatements
                .AnyAsync(s => s.OrderId == orderId);
        }
    }
}