using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using AMFINAV.AuthAPI.Application;
using AMFINAV.AuthAPI.Infrastructure;
using AMFINAV.AuthAPI.Infrastructure.Data;
using AMFINAV.AuthAPI.API.Middleware;
using AMFINAV.AuthAPI.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ───────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Clean Architecture Layers ─────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── JWT Authentication ────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSection["SecretKey"]!;
var issuer = jwtSection["Issuer"]!;
var audience = jwtSection["Audience"]!;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // ← Add this line — prevents ASP.NET Core from
        // remapping "role" to the long URI claim type
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
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

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                ctx.Response.Headers.Append(
                    "Token-Expired",
                    ctx.Exception is SecurityTokenExpiredException
                        ? "true" : "false");
                return Task.CompletedTask;
            }
        };
    });

// ── Authorization Policies ────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("role", "Admin"));

    options.AddPolicy("EmployeeOrAbove", policy =>
        policy.RequireClaim("role", "Admin", "Employee"));

    options.AddPolicy("AllRoles", policy =>
        policy.RequireAuthenticatedUser());

    // Permission-based policies
    options.AddPolicy("CanReadSchemes", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "Admin") ||
            ctx.User.HasClaim("permissions", "scheme.read")));

    options.AddPolicy("CanCreateSchemes", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "Admin") ||
            ctx.User.HasClaim("permissions", "scheme.create")));

    options.AddPolicy("CanUpdateSchemes", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "Admin") ||
            ctx.User.HasClaim("permissions", "scheme.update")));

    options.AddPolicy("CanApproveFunds", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "Admin") ||
            ctx.User.HasClaim("permissions", "fund.approval")));

    options.AddPolicy("CanReadNav", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "Admin") ||
            ctx.User.HasClaim("permissions", "nav.read")));
});

// ── Swagger with JWT Support ──────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AMFINAV Auth API",
        Version = "v1",
        Description = "Centralized Identity & Authentication Service for AMFINAV applications."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {your JWT token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// ── CORS (allow Gateway + future clients) ─────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// ── Middleware Pipeline (ORDER MATTERS) ───────────────────────────

// 1 — Global exception handler first
app.UseGlobalExceptionHandler();

// 2 — CORS
app.UseCors("AllowGateway");

// 3 — Auto-migrate + seed admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
await AdminSeedService.SeedAdminAsync(app.Services, app.Configuration);

// 4 — Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AMFINAV Auth API v1");
    c.DisplayRequestDuration();
});

// 5 — Auth
app.UseAuthentication();
app.UseAuthorization();

// 6 — Controllers
app.MapControllers();

app.Run();