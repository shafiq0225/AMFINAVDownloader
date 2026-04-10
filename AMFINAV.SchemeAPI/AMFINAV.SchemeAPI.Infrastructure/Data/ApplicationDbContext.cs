using Microsoft.EntityFrameworkCore;
using AMFINAV.SchemeAPI.Domain.Entities;

namespace AMFINAV.SchemeAPI.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<SchemeEnrollment> SchemeEnrollments { get; set; }
        public DbSet<DetailedScheme> DetailedSchemes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SchemeEnrollment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SchemeCode).IsUnique();
                entity.Property(e => e.SchemeCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.SchemeName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<DetailedScheme>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.SchemeCode, e.NavDate }).IsUnique();
                entity.HasIndex(e => e.FundCode);
                entity.Property(e => e.FundCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FundName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SchemeCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.SchemeName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Nav).HasColumnType("decimal(18,4)");
                entity.Property(e => e.ReceivedAt).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}