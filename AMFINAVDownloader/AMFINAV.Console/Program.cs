using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Quartz;
using AMFINAV.Application;
using AMFINAV.Infrastructure;
using AMFINAV.Infrastructure.Helpers;
using AMFINAV.Application.UseCases.Commands;
using AMFINAV.Console.Jobs;
using AMFINAV.Domain.Interfaces;
using MassTransit;

namespace AMFINAV.Console
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)  // ← Change this
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args, configuration).Build();

                // Ensure database is created
                using (var scope = host.Services.CreateScope())
                {
                    var holidayFetcher = scope.ServiceProvider.GetRequiredService<INseHolidayFetcher>();
                    DateHelper.Initialize(holidayFetcher);

                    // Optional: Pre-load holidays on startup
                    await holidayFetcher.FetchAllHolidaysAsync();


                    var dbContext = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.ApplicationDbContext>();

                    // ✅ Don't recreate DB, just verify connection
                    var canConnect = await dbContext.Database.CanConnectAsync();
                    if (canConnect)
                        Log.Information("Database ready");
                    else
                        Log.Error("Cannot connect to database!");
                }

                if (args.Length > 0 && args[0].ToLower() == "runonce")
                {
                    await RunOnceAsync(host.Services);
                }
                else
                {
                    Log.Information("Starting scheduled service - Will run daily at 5:00 AM");
                    await host.RunAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static async Task RunOnceAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var command = scope.ServiceProvider.GetRequiredService<DownloadAndStoreNavCommand>();

            var targetDate = DateHelper.GetTargetNavDate();
            Log.Information("Target NAV date: {Date}", targetDate.ToString("yyyy-MM-dd"));

            var result = await command.ExecuteAsync(targetDate);

            if (result.IsSuccess)
            {
                if (result.Data)
                    Log.Information("✅ Data downloaded and stored successfully");
                else
                    Log.Information("Data already exists for this date");
            }
            else
            {
                Log.Error("❌ Failed: {Error}", result.ErrorMessage);
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "AMFINAV NAV Downloader";
            })
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Add Application Layer
                    services.AddApplication();

                    // Add Infrastructure Layer
                    services.AddInfrastructure(configuration);

                    // ── MassTransit + RabbitMQ ─────────────────────────────
                    services.AddMassTransit(x =>
                    {
                        x.UsingRabbitMq((ctx, cfg) =>
                        {
                            cfg.Host(
                                configuration["RabbitMQ:Host"] ?? "localhost",
                                configuration["RabbitMQ:VirtualHost"] ?? "/",
                                h =>
                                {
                                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                                });
                        });
                    });

                    // Add Quartz Scheduling
                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionJobFactory();

                        var jobKey = new JobKey("NavDownloadJob");
                        q.AddJob<NavDownloadJob>(opts => opts.WithIdentity(jobKey));

                        var scheduleTime = configuration.GetValue<string>("AppSettings:ScheduleTime", "5:00:00");
                        var timeParts = scheduleTime.Split(':');
                        var hour = int.Parse(timeParts[0]);
                        var minute = int.Parse(timeParts[1]);

                        q.AddTrigger(opts => opts
                            .ForJob(jobKey)
                            .WithIdentity("NavDownloadTrigger")
                            .WithCronSchedule($"0 {minute} {hour} * * ?"));
                    });

                    services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
                });
    }
}