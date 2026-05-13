using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AMFINAV.Investment.API.Extensions
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("JwtSettings");
            var secretKey = jwtSection["SecretKey"]!;
            var issuer = jwtSection["Issuer"]!;
            var audience = jwtSection["Audience"]!;

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        // ← CRITICAL: prevents role claim remapping
                        options.MapInboundClaims = false;

                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(secretKey)),
                                ValidateIssuer = true,
                                ValidIssuer = issuer,
                                ValidateAudience = true,
                                ValidAudience = audience,
                                ValidateLifetime = true,
                                ClockSkew = TimeSpan.Zero
                            };
                    });

            // ── Authorization Policies ─────────────────────────────
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireClaim("role", "Admin"));

                options.AddPolicy("AdminOrEmployee", policy =>
                    policy.RequireAssertion(ctx =>
                        ctx.User.HasClaim("role", "Admin") ||
                        ctx.User.HasClaim("role", "Employee")));

                options.AddPolicy("AnyRole", policy =>
                    policy.RequireAssertion(ctx =>
                        ctx.User.HasClaim("role", "Admin") ||
                        ctx.User.HasClaim("role", "Employee") ||
                        ctx.User.HasClaim("role", "User")));
            });

            return services;
        }
    }
}