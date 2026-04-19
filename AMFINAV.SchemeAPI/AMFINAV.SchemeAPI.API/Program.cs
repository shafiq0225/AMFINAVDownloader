using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AMFINAV.SchemeAPI.Application;
using AMFINAV.SchemeAPI.Infrastructure;
using AMFINAV.SchemeAPI.Infrastructure.Consumers;
using AMFINAV.SchemeAPI.Infrastructure.Data;
using AMFINAV.SchemeAPI.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


// ── JWT Authentication ─────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSection["SecretKey"]!;
var issuer = jwtSection["Issuer"]!;
var audience = jwtSection["Audience"]!;

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AMFINAV SchemeAPI",
        Version = "v1",
        Description = "Scheme Enrollment, Fund Approval and NAV Comparison API"
    });

    // ← JWT Bearer definition
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

    c.AddSecurityDefinition(
        JwtBearerDefaults.AuthenticationScheme, securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});
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
        // ← Prevent claim remapping
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
    });

// ── Authorization Policies ─────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
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

// ── MassTransit ────────────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NavFileConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"] ?? "localhost",
            builder.Configuration["RabbitMQ:VirtualHost"] ?? "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });

        cfg.ReceiveEndpoint("nav-file-processed-app2", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Bind("AMFINAV.Domain.Contracts:NavFileProcessedEvent", b =>
            {
                b.ExchangeType = "fanout";
            });
            e.ConfigureConsumer<NavFileConsumer>(ctx);
        });
    });
});

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────
app.UseGlobalExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

// ← Auth before controllers
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();