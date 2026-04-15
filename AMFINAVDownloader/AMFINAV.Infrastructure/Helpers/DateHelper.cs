using AMFINAV.Domain.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AMFINAV.Infrastructure.Helpers
{
    public static class DateHelper
    {
        private static INseHolidayFetcher _holidayFetcher;
        private static bool _isInitialized = false;
        private static HashSet<DateTime> _cachedHolidays;

        public static void Initialize(INseHolidayFetcher holidayFetcher)
        {
            _holidayFetcher = holidayFetcher;
            _isInitialized = true;
        }

        public static async Task<DateTime> GetTargetNavDateAsync()
        {
            var today = DateTime.Today;
            var targetDate = today.AddDays(-1);

            // Skip weekends and holidays
            while (!await IsTradingDayAsync(targetDate))
            {
                Log.Information("No Marktet on the date " + targetDate.Date);
                targetDate = targetDate.AddDays(-1);
            }

            return targetDate;
        }

        public static async Task<bool> IsTradingDayAsync(DateTime date)
        {
            // Check weekend
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return false;

            // Check holidays using NSE API fetcher
            if (_isInitialized && _holidayFetcher != null)
            {
                try
                {
                    if (_cachedHolidays == null)
                    {
                        _cachedHolidays = await _holidayFetcher.FetchAllHolidaysAsync();
                    }
                    return !_cachedHolidays.Contains(date.Date);
                }
                catch (Exception ex)
                {
                    // Log error and assume it's a trading day if API fails
                    Console.WriteLine($"Failed to check holiday: {ex.Message}");
                    return true; // Assume trading day on API failure
                }
            }

            // If no holiday service, assume it's a trading day
            return true;
        }

        public static DateTime GetTargetNavDate()
        {
            return GetTargetNavDateAsync().GetAwaiter().GetResult();
        }
    }
}