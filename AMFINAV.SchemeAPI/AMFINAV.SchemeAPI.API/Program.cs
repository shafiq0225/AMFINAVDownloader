using AMFINAV.SchemeAPI.Application;
using AMFINAV.SchemeAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AMFINAV.SchemeAPI.Infrastructure.Data;
using MassTransit;
using AMFINAV.SchemeAPI.Infrastructure.Consumers;
using AMFINAV.SchemeAPI.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── MassTransit + RabbitMQ ─────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Register consumer
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

        // Configure receive endpoint (queue)
        cfg.ReceiveEndpoint("nav-file-processed-app2", e =>
        {
            // ← Bind to App 1's exchange explicitly
            e.Bind("AMFINAV.Domain.Contracts:NavFileProcessedEvent", b =>
            {
                b.ExchangeType = "fanout";
            });

            e.ConfigureConsumer<NavFileConsumer>(ctx);
        });
    });
});


var app = builder.Build();

app.UseGlobalExceptionHandler();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

app.Run();