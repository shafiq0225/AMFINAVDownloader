using Microsoft.Extensions.DependencyInjection;
using AMFINAV.Application.UseCases.Commands;

namespace AMFINAV.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register Use Cases
            services.AddScoped<DownloadAndStoreNavCommand>();
            return services;
        }
    }
}