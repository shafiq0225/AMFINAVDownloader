using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AMFINAV.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AMFINAV.Infrastructure.Services
{
    public class NseHolidayFetcher : INseHolidayFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NseHolidayFetcher> _logger;
        private readonly IConfiguration _configuration;
        private const string CACHE_KEY = "NSE_HOLIDAYS_ALL";

        public NseHolidayFetcher(HttpClient httpClient, IMemoryCache memoryCache, ILogger<NseHolidayFetcher> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = memoryCache;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<DateTime>> FetchHolidaysForYearAsync(int year)
        {
            var allHolidays = await FetchAllHolidaysAsync();
            return allHolidays.Where(h => h.Year == year).ToList();
        }

        public async Task<HashSet<DateTime>> FetchAllHolidaysAsync()
        {
            // Check cache first
            if (_cache.TryGetValue(CACHE_KEY, out HashSet<DateTime> cachedHolidays))
            {
                _logger.LogInformation("Returning {Count} holidays from cache", cachedHolidays.Count);
                return cachedHolidays;
            }

            try
            {
                var apiUrl = _configuration["AppSettings:NseHolidayApiUrl"];
                if (string.IsNullOrEmpty(apiUrl))
                {
                    _logger.LogWarning("NSE Holiday API URL not configured");
                    return new HashSet<DateTime>();
                }

                _logger.LogInformation("Fetching holidays from NSE API: {Url}", apiUrl);

                // Now fetch the holiday data
                var holidayResponse = await _httpClient.GetAsync(apiUrl);
                holidayResponse.EnsureSuccessStatusCode();

                var json = await holidayResponse.Content.ReadAsStringAsync();

                // Parse the JSON response
                using var doc = JsonDocument.Parse(json);

                var holidays = new HashSet<DateTime>();

                // Parse MF segment (Mutal Fund) - Main trading holidays
                if (doc.RootElement.TryGetProperty("MF", out var cmElement))
                {
                    var cmHolidays = ParseHolidaysFromJson(cmElement);
                    foreach (var holiday in cmHolidays)
                        holidays.Add(holiday);
                    _logger.LogInformation("Found {Count} holidays in CM segment", cmHolidays.Count);
                }
                // Cache for 24 hours
                _cache.Set(CACHE_KEY, holidays, TimeSpan.FromHours(24));

                _logger.LogInformation("Total holidays fetched: {Count}", holidays.Count);
                return holidays;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch holidays from NSE API");
                return new HashSet<DateTime>();
            }
        }

        private static List<DateTime> ParseHolidaysFromJson(JsonElement element)
        {
            var holidays = new List<DateTime>();

            foreach (var item in element.EnumerateArray())
            {
                if (item.TryGetProperty("tradingDate", out var dateProperty))
                {
                    var dateStr = dateProperty.GetString();
                    if (!string.IsNullOrEmpty(dateStr))
                    {
                        // NSE returns date in format "dd-MMM-yyyy" e.g., "15-Jan-2026"
                        if (DateTime.TryParseExact(dateStr, "dd-MMM-yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDate))
                        {
                            holidays.Add(exactDate.Date);
                        }
                        else if (DateTime.TryParse(dateStr, out var parsedDate))
                        {
                            holidays.Add(parsedDate.Date);
                        }
                    }
                }
            }

            return holidays;
        }

        public async Task RefreshHolidaysAsync()
        {
            _cache.Remove(CACHE_KEY);
            await FetchAllHolidaysAsync();
        }
    }
}