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

        /// <summary>
        /// Called by /daily endpoint.
        /// Automatically finds the last 2 dates that HAVE actual NAV data.
        /// Handles weekends, holidays, and missing data gaps correctly.
        /// </summary>
        public async Task<Result<NavComparisonResponseDto>> ExecuteDailyAsync()
        {
            // ← Get last 2 dates that actually have data in DetailedSchemes
            var tradingDates = await _unitOfWork.DetailedSchemes.GetLastTradingDatesAsync(2);

            if (tradingDates.Count == 0)
                return Result<NavComparisonResponseDto>.Failure("No NAV data found in DetailedSchemes.");

            if (tradingDates.Count == 1)
            {
                // Only one date available — use it as both start and end
                var only = tradingDates[0].Date;
                return await ExecuteAsync(only, only);
            }

            // tradingDates[0] = most recent, tradingDates[1] = previous
            var endDate = tradingDates[0].Date;
            var startDate = tradingDates[1].Date;

            _logger.LogInformation("Daily comparison — last 2 trading dates: {Start} and {End}", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            return await ExecuteAsync(startDate, endDate);
        }

        public async Task<Result<NavComparisonResponseDto>> ExecuteAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("NAV comparison: {Start} to {End}", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                // ← Use new method that includes previous record for calculation
                var records = await _unitOfWork.DetailedSchemes.GetByDateRangeWithPreviousAsync(startDate, endDate);

                var recordList = records.ToList();

                if (recordList.Count == 0)
                    return Result<NavComparisonResponseDto>.Failure($"No NAV data found between " + $"{startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}.");

                var grouped = recordList.GroupBy(r => r.SchemeCode).ToList();
                var schemes = new List<SchemeComparisonDto>();

                foreach (var group in grouped)
                {
                    var navByDate = group.ToDictionary(r => r.NavDate.Date, r => r.Nav);

                    var orderedDates = navByDate.Keys.OrderBy(d => d).ToList();

                    var history = new List<NavHistoryDto>();

                    foreach (var date in orderedDates)
                    {
                        var currentNav = navByDate[date];

                        // Find previous date NAV (may be outside requested range)
                        var previousDate = orderedDates.Where(d => d < date).OrderByDescending(d => d).FirstOrDefault();

                        string percentage;
                        bool isGrowth;

                        if (previousDate == default || !navByDate.ContainsKey(previousDate))
                        {
                            // Truly no previous data exists anywhere
                            percentage = "100.00";
                            isGrowth = true;
                        }
                        else
                        {
                            var previousNav = navByDate[previousDate];
                            if (previousNav == 0)
                            {
                                percentage = "100.00";
                                isGrowth = true;
                            }
                            else
                            {
                                var change = ((currentNav - previousNav) / previousNav) * 100;
                                percentage = change.ToString("F2");
                                isGrowth = currentNav > previousNav;
                            }
                        }

                        // ← Only include dates within the requested range in output
                        // Previous record was fetched for calculation only
                        if (date >= startDate.Date)
                        {
                            history.Add(new NavHistoryDto
                            {
                                Date = date,
                                Nav = currentNav,
                                Percentage = percentage,
                                IsTradingHoliday = false,
                                IsGrowth = isGrowth
                            });
                        }
                    }

                    var first = group.OrderBy(r => r.NavDate).First(r => r.NavDate.Date >= startDate.Date);

                    schemes.Add(new SchemeComparisonDto
                    {
                        FundName = first.FundName,
                        SchemeCode = first.SchemeCode,
                        SchemeName = first.SchemeName,
                        History = history
                    });
                }

                // Rank by latest date percentage descending
                var latestDate = endDate.Date;

                var ranked = schemes
                    .OrderByDescending(s =>
                    {
                        var latest = s.History
                            .FirstOrDefault(h => h.Date.Date == latestDate);
                        if (latest == null) return decimal.MinValue;
                        return decimal.TryParse(latest.Percentage, out var p)
                            ? p : decimal.MinValue;
                    })
                    .ToList();

                for (int i = 0; i < ranked.Count; i++)
                    ranked[i].Rank = i + 1;

                return Result<NavComparisonResponseDto>.Success(new NavComparisonResponseDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Message = $"Retrieved {ranked.Count} scheme(s) successfully.",
                    Schemes = ranked
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNavComparisonQuery");
                return Result<NavComparisonResponseDto>.Failure(ex.Message);
            }
        }
    }
}