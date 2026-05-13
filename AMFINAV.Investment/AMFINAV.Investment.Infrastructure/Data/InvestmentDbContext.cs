using AMFINAV.Investment.Domain.Entities;
using AMFINAV.Investment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AMFINAV.Investment.Infrastructure.Data
{
    public class InvestmentDbContext : DbContext
    {
        public InvestmentDbContext(DbContextOptions<InvestmentDbContext> options)
            : base(options) { }

        public DbSet<InvestmentOrder> InvestmentOrders { get; set; }
        public DbSet<Holding> Holdings { get; set; }
        public DbSet<PortfolioSnapshot> PortfolioSnapshots { get; set; }
        public DbSet<InvestmentStatement> InvestmentStatements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(InvestmentDbContext).Assembly);
        }
    }
}