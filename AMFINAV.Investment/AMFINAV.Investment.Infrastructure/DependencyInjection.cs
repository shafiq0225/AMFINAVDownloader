using AMFINAV.Investment.Domain.Interfaces;
using AMFINAV.Investment.Infrastructure.Data;
using AMFINAV.Investment.Infrastructure.Repositories;
using AMFINAV.Investment.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AMFINAV.Investment.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Database ───────────────────────────────────────────
            services.AddDbContext<InvestmentDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly(
                        typeof(InvestmentDbContext).Assembly.FullName)));

            // ── Unit of Work + Repositories ────────────────────────
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ── NAV Rate Service ───────────────────────────────────
            services.AddScoped<INavRateService, NavRateService>();

            // ── Storage Service ────────────────────────────────────
            var azureConnectionString =
                configuration["AzureStorage:ConnectionString"];

            if (string.IsNullOrWhiteSpace(azureConnectionString))
            {
                services.AddScoped<IBlobStorageService,
                    LocalFileStorageService>();
            }
            else
            {
                services.AddScoped<IBlobStorageService,
                    BlobStorageService>();
            }

            return services;
        }
    }
}