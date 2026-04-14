using System.Globalization;
using MassTransit;
using Microsoft.Extensions.Logging;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Helpers;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using AMFINAV.Domain.Contracts;
using AMFINAV.SchemeAPI.Domain.Exceptions;

namespace AMFINAV.SchemeAPI.Infrastructure.Consumers
{
    public class NavFileConsumer : IConsumer<NavFileProcessedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NavFileConsumer> _logger;

        public NavFileConsumer(IUnitOfWork unitOfWork,
            ILogger<NavFileConsumer> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<NavFileProcessedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "📥 Received NavFileProcessedEvent for {Date} with {Count} records",
                message.NavDate.ToString("yyyy-MM-dd"), message.RecordCount);

            try
            {
                var approvedSchemes = await _unitOfWork.SchemeEnrollments
                    .GetApprovedSchemesAsync();

                var approvedCodes = approvedSchemes
                    .Select(s => s.SchemeCode)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (approvedCodes.Count == 0)
                {
                    _logger.LogWarning(
                        "No approved schemes found for {Date}. Skipping.",
                        message.NavDate.ToString("yyyy-MM-dd"));
                    return;
                }

                var lines = message.FileContent.Split(
                    new[] { '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries);

                var toInsert = new List<DetailedScheme>();
                var receivedAt = DateTime.Now;
                string currentFundName = string.Empty;
                string currentFundCode = string.Empty;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;

                    if (!trimmed.Contains(';'))
                    {
                        currentFundName = trimmed;
                        currentFundCode = FundCodeGenerator.Generate(currentFundName);
                        _logger.LogInformation(
                            "Fund detected: {FundName} → {FundCode}",
                            currentFundName, currentFundCode);
                        continue;
                    }

                    var parts = trimmed.Split(';');
                    if (parts.Length < 6) continue;

                    var schemeCode = parts[0].Trim();
                    if (!approvedCodes.Contains(schemeCode)) continue;

                    if (await _unitOfWork.DetailedSchemes
                            .ExistsBySchemeCodeAndDateAsync(schemeCode, message.NavDate))
                    {
                        _logger.LogInformation(
                            "Already exists — SchemeCode={Code} NavDate={Date}",
                            schemeCode, message.NavDate.ToString("yyyy-MM-dd"));
                        continue;
                    }

                    if (!decimal.TryParse(parts[4].Trim(),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var nav))
                    {
                        _logger.LogWarning(
                            "Invalid NAV value for SchemeCode={Code} — skipping",
                            schemeCode);
                        continue;
                    }

                    var enrollment = approvedSchemes
                        .First(s => s.SchemeCode == schemeCode);

                    toInsert.Add(new DetailedScheme
                    {
                        FundCode = currentFundCode,
                        FundName = currentFundName,
                        SchemeCode = schemeCode,
                        SchemeName = parts[3].Trim(),
                        IsApproved = enrollment.IsApproved,
                        Nav = nav,
                        NavDate = message.NavDate.Date,
                        ReceivedAt = receivedAt
                    });
                }

                if (toInsert.Count == 0)
                {
                    _logger.LogInformation(
                        "No new DetailedScheme records to insert for {Date}.",
                        message.NavDate.ToString("yyyy-MM-dd"));
                    return;
                }

                await _unitOfWork.DetailedSchemes.AddRangeAsync(toInsert);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "✅ Inserted {Count} records into DetailedScheme for {Date}",
                    toInsert.Count, message.NavDate.ToString("yyyy-MM-dd"));
            }
            catch (Exception ex)
            {
                // ← Wrap in typed exception with context
                _logger.LogError(ex,
                    "❌ NavFileConsumer failed for NavDate={Date} — " +
                    "Message will be retried by MassTransit",
                    message.NavDate.ToString("yyyy-MM-dd"));

                // Rethrow — MassTransit will retry automatically
                throw new NavConsumerException(
                    $"Failed to process NAV file for {message.NavDate:yyyy-MM-dd}",
                    message.NavDate,
                    ex);
            }
        }
    }
}