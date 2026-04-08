using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AMFINAV.Domain.Interfaces;
using AMFINAV.Infrastructure.Data;
using AMFINAV.Infrastructure.Repositories;
using AMFINAV.Infrastructure.Services;

namespace AMFINAV.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<INavFileRepository, NavFileRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddHttpClient<INavDownloadService, NavDownloadService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                client.DefaultRequestHeaders.Add("User-Agent", "AMFINAV-Downloader/1.0");
            });

            // Add NSE Holiday Fetcher with cookie container support
            services.AddHttpClient<INseHolidayFetcher, NseHolidayFetcher>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                client.DefaultRequestHeaders.Add("Referer", "https://www.nseindia.com/");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            });

            // Add memory cache
            services.AddMemoryCache();

            return services;
        }
    }
}