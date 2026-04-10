using System.Globalization;
using MassTransit;
using Microsoft.Extensions.Logging;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Helpers;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using AMFINAV.Domain.Contracts;

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
                // Step 1 — Fetch approved scheme codes
                var approvedSchemes = await _unitOfWork.SchemeEnrollments
                    .GetApprovedSchemesAsync();

                var approvedCodes = approvedSchemes
                    .Select(s => s.SchemeCode)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (approvedCodes.Count == 0)
                {
                    _logger.LogWarning("No approved schemes. Skipping.");
                    return;
                }

                _logger.LogInformation(
                    "Found {Count} approved scheme codes", approvedCodes.Count);

                // Step 2 — Parse NAV file
                // Format:
                //   Fund Name Line        ← header (no semicolons)
                //   [blank line]
                //   SchemeCode;ISIN1;ISIN2;SchemeName;NAV;Date
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

                    // Detect fund name header line
                    // Header lines have no semicolons
                    if (!trimmed.Contains(';'))
                    {
                        currentFundName = trimmed;
                        currentFundCode = FundCodeGenerator.Generate(currentFundName);
                        _logger.LogInformation(
                            "Fund detected: {FundName} → {FundCode}",
                            currentFundName, currentFundCode);
                        continue;
                    }

                    // Parse scheme line
                    var parts = trimmed.Split(';');
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

                    // Parse NAV
                    if (!decimal.TryParse(parts[4].Trim(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var nav))
                    {
                        _logger.LogWarning(
                            "Invalid NAV for SchemeCode={Code}", schemeCode);
                        continue;
                    }

                    // Get IsApproved from SchemeEnrollment
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
                _logger.LogError(ex, "Error in NavFileConsumer");
                throw;
            }
        }
    }
}