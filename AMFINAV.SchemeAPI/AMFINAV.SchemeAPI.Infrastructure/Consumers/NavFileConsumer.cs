using System.Globalization;
using MassTransit;
using Microsoft.Extensions.Logging;
using AMFINAV.SchemeAPI.Domain.Contracts;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Helpers;
using AMFINAV.SchemeAPI.Domain.Interfaces;

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
                // Step 1 — Fetch approved scheme codes from SchemeEnrollment table
                var approvedSchemes = await _unitOfWork.SchemeEnrollments
                    .GetApprovedSchemesAsync();

                var approvedCodes = approvedSchemes
                    .Select(s => s.SchemeCode)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (approvedCodes.Count == 0)
                {
                    _logger.LogWarning("No approved schemes found. Skipping DetailedScheme.");
                    return;
                }

                _logger.LogInformation(
                    "Found {Count} approved scheme codes", approvedCodes.Count);

                // Step 2 — Parse NAV file content
                var lines = message.FileContent.Split(
                    new[] { '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries);

                var toInsert = new List<DetailedScheme>();
                var receivedAt = DateTime.Now;

                foreach (var line in lines)
                {
                    var parts = line.Split(';');

                    // Format: SchemeCode;ISIN1;ISIN2;SchemeName;NAV;Date
                    if (parts.Length < 6) continue;

                    var schemeCode = parts[0].Trim();

                    // Only approved schemes
                    if (!approvedCodes.Contains(schemeCode)) continue;

                    // Skip duplicates
                    if (await _unitOfWork.DetailedSchemes
                            .ExistsBySchemeCodeAndDateAsync(schemeCode, message.NavDate))
                    {
                        _logger.LogInformation(
                            "Already exists — SchemeCode={Code} NavDate={Date}",
                            schemeCode, message.NavDate.ToString("yyyy-MM-dd"));
                        continue;
                    }

                    // Parse NAV value
                    if (!decimal.TryParse(parts[4].Trim(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var nav))
                    {
                        _logger.LogWarning(
                            "Invalid NAV value for SchemeCode={Code}", schemeCode);
                        continue;
                    }

                    var schemeName = parts[3].Trim();
                    var fundName = FundNameExtractor.Extract(schemeName);
                    var fundCode = FundCodeGenerator.Generate(fundName);

                    // Get IsApproved from SchemeEnrollment
                    var enrollment = approvedSchemes
                        .First(s => s.SchemeCode == schemeCode);

                    toInsert.Add(new DetailedScheme
                    {
                        FundCode = fundCode,
                        FundName = fundName,
                        SchemeCode = schemeCode,
                        SchemeName = schemeName,
                        IsApproved = enrollment.IsApproved,
                        Nav = nav,
                        NavDate = message.NavDate.Date,
                        ReceivedAt = receivedAt
                    });
                }

                if (toInsert.Count == 0)
                {
                    _logger.LogInformation("No new DetailedScheme records to insert.");
                    return;
                }

                // Step 3 — Bulk insert
                await _unitOfWork.DetailedSchemes.AddRangeAsync(toInsert);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "✅ Inserted {Count} records into DetailedScheme for {Date}",
                    toInsert.Count, message.NavDate.ToString("yyyy-MM-dd"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing NavFileProcessedEvent");
                throw; // MassTransit will retry
            }
        }
    }
}