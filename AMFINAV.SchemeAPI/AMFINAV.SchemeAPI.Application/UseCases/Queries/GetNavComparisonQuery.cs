using AMFINAV.SchemeAPI.Application.DTOs;
using AMFINAV.SchemeAPI.Domain.Common;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.SchemeAPI.Application.UseCases.Queries
{
    public class GetNavComparisonQuery
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetNavComparisonQuery> _logger;

        public GetNavComparisonQuery(IUnitOfWork unitOfWork,
            ILogger<GetNavComparisonQuery> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<NavComparisonResponseDto>> ExecuteAsync(
            DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation(
                    "NAV comparison requested: {Start} to {End}",
                    startDate.ToString("yyyy-MM-dd"),
                    endDate.ToString("yyyy-MM-dd"));

                // Step 1 — Fetch all DetailedScheme records in date range
                var records = await _unitOfWork.DetailedSchemes
                    .GetByDateRangeAsync(startDate, endDate);

                var recordList = records.ToList();

                if (recordList.Count == 0)
                    return Result<NavComparisonResponseDto>.Failure(
                        $"No NAV data found between " +
                        $"{startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}.");

                // Step 2 — Get all distinct dates in range (for holiday detection)
                var allDates = recordList
                    .Select(r => r.NavDate.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                // Step 3 — Group by SchemeCode
                var grouped = recordList
                    .GroupBy(r => r.SchemeCode)
                    .ToList();

                var schemes = new List<SchemeComparisonDto>();

                foreach (var group in grouped)
                {
                    var navByDate = group
                        .ToDictionary(r => r.NavDate.Date, r => r.Nav);

                    var orderedDates = navByDate.Keys
                        .OrderBy(d => d)
                        .ToList();

                    var history = new List<NavHistoryDto>();

                    foreach (var date in orderedDates)
                    {
                        var currentNav = navByDate[date];

                        // Find previous trading day NAV
                        var previousDate = orderedDates
                            .Where(d => d < date)
                            .OrderByDescending(d => d)
                            .FirstOrDefault();

                        decimal percentage = 0;
                        bool isGrowth = false;

                        if (previousDate != default && navByDate.ContainsKey(previousDate))
                        {
                            var previousNav = navByDate[previousDate];
                            if (previousNav != 0)
                            {
                                percentage = ((currentNav - previousNav) / previousNav) * 100;
                                isGrowth = currentNav > previousNav;
                            }
                        }

                        history.Add(new NavHistoryDto
                        {
                            Date = date,
                            Nav = currentNav,
                            Percentage = percentage.ToString("F2"),
                            IsTradingHoliday = false,
                            IsGrowth = isGrowth
                        });
                    }

                    var first = group.First();

                    schemes.Add(new SchemeComparisonDto
                    {
                        FundName = first.FundName,
                        SchemeCode = first.SchemeCode,
                        SchemeName = first.SchemeName,
                        History = history
                    });
                }

                // Step 4 — Rank by latest date percentage descending
                var latestDate = allDates.Last();

                var ranked = schemes
                    .OrderByDescending(s =>
                    {
                        var latest = s.History
                            .FirstOrDefault(h => h.Date.Date == latestDate);
                        return latest != null
                            ? decimal.Parse(latest.Percentage)
                            : decimal.MinValue;
                    })
                    .ToList();

                for (int i = 0; i < ranked.Count; i++)
                    ranked[i].Rank = i + 1;

                var response = new NavComparisonResponseDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Message = $"Retrieved {ranked.Count} scheme(s) successfully.",
                    Schemes = ranked
                };

                _logger.LogInformation(
                    "NAV comparison completed — {Count} schemes returned",
                    ranked.Count);

                return Result<NavComparisonResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNavComparisonQuery");
                return Result<NavComparisonResponseDto>.Failure(ex.Message);
            }
        }
    }
}