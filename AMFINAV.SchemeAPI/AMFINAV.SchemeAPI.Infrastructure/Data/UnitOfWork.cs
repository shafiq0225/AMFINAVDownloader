using AMFINAV.SchemeAPI.Domain.Interfaces;
using AMFINAV.SchemeAPI.Infrastructure.Repositories;

namespace AMFINAV.SchemeAPI.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            SchemeEnrollments = new SchemeEnrollmentRepository(_context);
        }

        public ISchemeEnrollmentRepository SchemeEnrollments { get; }

        public async Task<int> CompleteAsync() =>
            await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}