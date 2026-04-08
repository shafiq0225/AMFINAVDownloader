using Microsoft.EntityFrameworkCore;
using AMFINAV.Domain.Entities;

namespace AMFINAV.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public DbSet<NavFile> NavFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NavFile>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Unique constraint on NavDate to prevent duplicates
                entity.HasIndex(e => e.NavDate).IsUnique();

                entity.Property(e => e.DownloadedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.FileContent).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Checksum).HasMaxLength(64);
            });
        }
    }
}