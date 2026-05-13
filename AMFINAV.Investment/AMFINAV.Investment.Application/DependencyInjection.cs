using AMFINAV.Investment.Application.Orders.Commands;
using AMFINAV.Investment.Application.Orders.Queries;
using AMFINAV.Investment.Application.Portfolio.Commands;
using AMFINAV.Investment.Application.Portfolio.Queries;
using AMFINAV.Investment.Application.Statements.Commands;
using AMFINAV.Investment.Application.Statements.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace AMFINAV.Investment.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            // ── Order Commands ─────────────────────────────────────
            services.AddScoped<CreateOrderCommand>();
            services.AddScoped<UpdateOrderStatusCommand>();

            // ── Order Queries ──────────────────────────────────────
            services.AddScoped<GetAllOrdersQuery>();
            services.AddScoped<GetOrderByIdQuery>();

            // ── Portfolio Commands ─────────────────────────────────
            services.AddScoped<CalculateSnapshotCommand>();

            // ── Portfolio Queries ──────────────────────────────────
            services.AddScoped<GetPortfolioQuery>();
            services.AddScoped<GetFamilyPortfolioQuery>();
            services.AddScoped<GetAllHoldingsQuery>();

            // ── Statement Commands ─────────────────────────────────
            services.AddScoped<UploadStatementCommand>();

            // ── Statement Queries ──────────────────────────────────
            services.AddScoped<GetStatementsQuery>();
            services.AddScoped<DownloadStatementQuery>();

            return services;
        }
    }
}