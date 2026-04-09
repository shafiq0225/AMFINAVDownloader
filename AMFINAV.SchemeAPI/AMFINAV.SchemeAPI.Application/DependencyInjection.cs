using Microsoft.Extensions.DependencyInjection;
using AMFINAV.SchemeAPI.Application.UseCases.Commands;
using AMFINAV.SchemeAPI.Application.UseCases.Queries;

namespace AMFINAV.SchemeAPI.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CreateSchemeEnrollmentCommand>();
            services.AddScoped<UpdateSchemeEnrollmentCommand>();
            services.AddScoped<GetSchemeEnrollmentsQuery>();
            return services;
        }
    }
}