using System.Text.Json;
using Azure.Messaging.ServiceBus;
using AMFINAV.Domain.Contracts;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AMFINAV.SchemeAPI.Domain.Interfaces;

namespace AMFINAV.SchemeAPI.Infrastructure.Consumers
{
    public class MarketHolidayConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MarketHolidayConsumer> _logger;
        private readonly IConfiguration _configuration;

        private ServiceBusProcessor? _processor;

        public MarketHolidayConsumer(
            IServiceScopeFactory scopeFactory,
            ILogger<MarketHolidayConsumer> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = _configuration["AzureServiceBus:ConnectionString"];
            var topicName = _configuration["AzureServiceBus:HolidayTopicName"]
                                   ?? "amfi-market-holidays";
            var subscriptionName = _configuration["AzureServiceBus:SubscriptionName"]
                                   ?? "sub-app2-scheme";

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning(
                    "AzureServiceBus:ConnectionString not configured — " +
                    "MarketHolidayConsumer will not start.");
                return;
            }

            var client = new ServiceBusClient(connectionString);

            _processor = client.CreateProcessor(topicName, subscriptionName,
                new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = 1,
                    AutoCompleteMessages = false
                });

            _processor.ProcessMessageAsync += OnMessageReceivedAsync;
            _processor.ProcessErrorAsync += OnErrorAsync;

            await _processor.StartProcessingAsync(stoppingToken);

            _logger.LogInformation(
                "📡 MarketHolidayConsumer started — " +
                "Topic: {Topic}, Subscription: {Sub}",
                topicName, subscriptionName);

            // Keep alive until host stops
            await Task.Delay(Timeout.Infinite, stoppingToken)
                      .ContinueWith(_ => { }, CancellationToken.None);

            await _processor.StopProcessingAsync();
        }

        private async Task OnMessageReceivedAsync(
            ProcessMessageEventArgs args)
        {
            try
            {
                var body = args.Message.Body.ToString();
                _logger.LogInformation(
                    "📥 MarketHolidayConsumer received message: {Body}", body);

                var holidayEvent = JsonSerializer.Deserialize<MarketHolidayEvent>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (holidayEvent is null)
                {
                    _logger.LogWarning("Failed to deserialize MarketHolidayEvent — skipping");
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider
                    .GetRequiredService<IUnitOfWork>();

                var holidayDate = holidayEvent.HolidayDate.Date;

                if (await unitOfWork.MarketHolidays.ExistsByDateAsync(holidayDate))
                {
                    _logger.LogInformation(
                        "MarketHoliday already stored for {Date} — skipping",
                        holidayDate.ToString("yyyy-MM-dd"));
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                await unitOfWork.MarketHolidays.AddAsync(new MarketHoliday
                {
                    HolidayDate = holidayDate,
                    Source = holidayEvent.Source,
                    ReceivedAt = DateTime.UtcNow
                });

                await unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "✅ MarketHoliday stored for {Date}",
                    holidayDate.ToString("yyyy-MM-dd"));

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to process MarketHolidayEvent — " +
                    "message will be retried");

                // Abandon — Service Bus will retry up to MaxDeliveryCount
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task OnErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception,
                "❌ MarketHolidayConsumer error — Source: {Source}",
                args.ErrorSource);
            return Task.CompletedTask;
        }
    }
}