using System.Text.Json;
using Azure.Messaging.ServiceBus;
using AMFINAV.Domain.Contracts;
using AMFINAV.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AMFINAV.Infrastructure.Services
{
    public class AzureServiceBusHolidayPublisher : IHolidayEventPublisher, IAsyncDisposable
    {
        private readonly ServiceBusSender _sender;
        private readonly ILogger<AzureServiceBusHolidayPublisher> _logger;

        public AzureServiceBusHolidayPublisher(
            ServiceBusClient client,
            IConfiguration configuration,
            ILogger<AzureServiceBusHolidayPublisher> logger)
        {
            var topicName = configuration["AzureServiceBus:HolidayTopicName"]
                            ?? "amfi-market-holidays";
            _sender = client.CreateSender(topicName);
            _logger = logger;
        }

        public async Task PublishAsync(MarketHolidayEvent holidayEvent)
        {
            var json = JsonSerializer.Serialize(holidayEvent);

            var message = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = nameof(MarketHolidayEvent),

                // Deduplication: Service Bus won't re-deliver if the same
                // MessageId arrives within the dedup window (must be enabled
                // on the topic in Azure Portal → "Requires duplicate detection")
                MessageId = $"holiday-{holidayEvent.HolidayDate:yyyy-MM-dd}"
            };

            await _sender.SendMessageAsync(message);

            _logger.LogInformation(
                "📤 Published MarketHolidayEvent for {Date} to Azure Service Bus",
                holidayEvent.HolidayDate.ToString("yyyy-MM-dd"));
        }

        public async ValueTask DisposeAsync() => await _sender.DisposeAsync();
    }
}