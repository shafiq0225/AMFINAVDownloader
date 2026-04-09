using Microsoft.EntityFrameworkCore;
using AMFINAV.SchemeAPI.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AMFINAV.SchemeAPI.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public DbSet<SchemeEnrollment> SchemeEnrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SchemeEnrollment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.SchemeCode)
                      .IsUnique();

                entity.Property(e => e.SchemeCode)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(e => e.SchemeName)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}