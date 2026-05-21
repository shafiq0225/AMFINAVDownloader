using AMFINAV.Investment.Application.Family.Dtos;
using AMFINAV.Investment.Domain.Common;
using AMFINAV.Investment.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AMFINAV.Investment.Application.Family.Queries
{
    public class FamilyPortfolioQuery
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _connString;
        private readonly ILogger<FamilyPortfolioQuery> _logger;

        public FamilyPortfolioQuery(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<FamilyPortfolioQuery> logger)
        {
            _unitOfWork = unitOfWork;
            _connString = configuration
                .GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        // ── Screen 1: Family Overview ──────────────────────────────
        public async Task<Result<FamilyOverviewDto>> GetFamilyOverviewAsync()
        {
            try
            {
                var holdings = await _unitOfWork.Holdings
                    .GetAllActiveGroupedAsync();

                var holdingList = holdings.ToList();

                if (!holdingList.Any())
                    return Result<FamilyOverviewDto>.Success(
                        new FamilyOverviewDto
                        {
                            ReportDate = DateTime.Today
                        });

                // Get latest snapshots for all holdings
                var snapshotMap = await GetLatestSnapshotsAsync(
                    holdingList.Select(h => h.Id).ToList());

                // Group by investor
                var memberGroups = holdingList
                    .GroupBy(h => new
                    {
                        h.InvestorUserId,
                        h.InvestorName
                    })
                    .ToList();

                var members = new List<MemberSummaryDto>();

                foreach (var group in memberGroups)
                {
                    decimal invested = 0, currentValue = 0;
                    var schemes = new HashSet<string>();

                    foreach (var h in group)
                    {
                        invested += h.InvestedAmount;
                        schemes.Add(h.SchemeCode);

                        if (snapshotMap.TryGetValue(h.Id, out var snap))
                            currentValue += snap.currentValue;
                        else
                            currentValue += h.InvestedAmount;
                    }

                    var gain = currentValue - invested;
                    var gainPct = invested > 0
                        ? Math.Round((gain / invested) * 100, 4) : 0;

                    members.Add(new MemberSummaryDto
                    {
                        InvestorUserId = group.Key.InvestorUserId,
                        InvestorName = group.Key.InvestorName,
                        Initials = GetInitials(group.Key.InvestorName),
                        TotalInvested = invested,
                        TotalCurrentValue = currentValue,
                        TotalGain = Math.Round(gain, 2),
                        TotalGainPercent = gainPct,
                        IsGain = gain >= 0,
                        SchemeCount = schemes.Count,
                        HoldingCount = group.Count(),
                        CategorySummary = GetCategorySummary(group.ToList())
                    });
                }

                var totalInvested = members.Sum(m => m.TotalInvested);
                var totalValue = members.Sum(m => m.TotalCurrentValue);
                var totalGain = totalValue - totalInvested;
                var totalGainPct = totalInvested > 0
                    ? Math.Round((totalGain / totalInvested) * 100, 4) : 0;

                return Result<FamilyOverviewDto>.Success(
                    new FamilyOverviewDto
                    {
                        TotalFamilyInvested = totalInvested,
                        TotalFamilyCurrentValue = totalValue,
                        TotalFamilyGain = Math.Round(totalGain, 2),
                        TotalFamilyGainPercent = totalGainPct,
                        IsFamilyGain = totalGain >= 0,
                        TotalMembers = members.Count,
                        TotalSchemes = holdingList
                            .Select(h => h.SchemeCode).Distinct().Count(),
                        ReportDate = DateTime.Today,
                        Members = members.OrderBy(m => m.InvestorName)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting family overview");
                return Result<FamilyOverviewDto>
                    .Failure($"Failed: {ex.Message}");
            }
        }

        // ── Screen 2: Member Holdings ──────────────────────────────
        public async Task<Result<MemberHoldingsDto>> GetMemberHoldingsAsync(
            string investorUserId)
        {
            try
            {
                var holdings = await _unitOfWork.Holdings
                    .GetAllByInvestorActiveAsync(investorUserId);

                var holdingList = holdings.ToList();

                if (!holdingList.Any())
                    return Result<MemberHoldingsDto>.Failure(
                        "No active holdings found for this investor.");

                // Latest snapshot + NAV history per scheme
                var snapshotMap = await GetLatestSnapshotsAsync(
                    holdingList.Select(h => h.Id).ToList());

                var navHistoryMap = await GetNavHistoryAsync(
                    holdingList.Select(h => h.SchemeCode).Distinct().ToList());

                var cards = new List<HoldingCardDto>();

                foreach (var h in holdingList)
                {
                    var hasSnap = snapshotMap.TryGetValue(h.Id, out var snap);
                    var currentNAV = hasSnap ? snap.currentNAV : 0;
                    var currentValue = hasSnap ? snap.currentValue : h.InvestedAmount;
                    var gain = currentValue - h.InvestedAmount;
                    var gainPct = h.InvestedAmount > 0
                        ? Math.Round((gain / h.InvestedAmount) * 100, 4) : 0;

                    navHistoryMap.TryGetValue(h.SchemeCode,
                        out var navHistory);

                    cards.Add(new HoldingCardDto
                    {
                        HoldingId = h.Id,
                        SchemeCode = h.SchemeCode,
                        SchemeName = h.SchemeName,
                        FundName = h.FundName,
                        FolioNumber = h.FolioNumber,
                        OrderNumber = h.Order?.OrderNumber ?? string.Empty,

                        InvestedAmount = h.InvestedAmount,
                        Units = h.Units,
                        PurchaseNAV = h.PurchaseNAV,
                        CurrentNAV = currentNAV,
                        CurrentValue = Math.Round(currentValue, 2),
                        Gain = Math.Round(gain, 2),
                        GainPercent = gainPct,
                        IsGain = gain >= 0,

                        DayBefore = CalcQuickReturn(
                            "D-2", 2, navHistory, currentNAV),
                        OneMonth = CalcQuickReturn(
                            "1M", 30, navHistory, currentNAV),
                        SixMonth = CalcQuickReturn(
                            "6M", 180, navHistory, currentNAV),
                        OneYear = CalcQuickReturn(
                            "1Y", 365, navHistory, currentNAV)
                    });
                }

                var totalInvested = cards.Sum(c => c.InvestedAmount);
                var totalValue = cards.Sum(c => c.CurrentValue);
                var totalGain = totalValue - totalInvested;
                var totalGainPct = totalInvested > 0
                    ? Math.Round((totalGain / totalInvested) * 100, 4) : 0;

                var first = holdingList.First();

                return Result<MemberHoldingsDto>.Success(
                    new MemberHoldingsDto
                    {
                        InvestorUserId = first.InvestorUserId,
                        InvestorName = first.InvestorName,
                        Initials = GetInitials(first.InvestorName),
                        TotalInvested = totalInvested,
                        TotalCurrentValue = totalValue,
                        TotalGain = Math.Round(totalGain, 2),
                        TotalGainPercent = totalGainPct,
                        IsGain = totalGain >= 0,
                        Holdings = cards.OrderBy(c => c.SchemeName)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting holdings for {UserId}", investorUserId);
                return Result<MemberHoldingsDto>
                    .Failure($"Failed: {ex.Message}");
            }
        }

        // ── Screen 3: Scheme Detail ────────────────────────────────
        public async Task<Result<HoldingSchemeDetailDto>> GetSchemeDetailAsync(
            string investorUserId,
            string schemeCode)
        {
            try
            {
                var holdings = await _unitOfWork.Holdings
                    .GetAllByInvestorActiveAsync(investorUserId);

                var holding = holdings
                    .FirstOrDefault(h => h.SchemeCode == schemeCode);

                if (holding == null)
                    return Result<HoldingSchemeDetailDto>.Failure(
                        $"No holding found for scheme {schemeCode}.");

                // Latest snapshot
                var snapshot = await _unitOfWork.Portfolio
                    .GetLatestByHoldingAsync(holding.Id);

                var currentNAV = snapshot?.CurrentNAV ?? 0;
                var currentValue = snapshot?.CurrentValue ?? holding.InvestedAmount;

                // NAV history for this scheme
                var navHistoryMap = await GetNavHistoryAsync(new List<string> { schemeCode });
                navHistoryMap.TryGetValue(schemeCode, out var navHistory);

                var navList = navHistory ?? new List<NavRecord>();

                // Previous NAV
                var previousNav = navList.Count >= 2 ? navList[1].NAV : 0;
                var previousNavDate = navList.Count >= 2
                    ? navList[1].NavDate : DateTime.MinValue;
                var dailyChange = currentNAV > 0 && previousNav > 0
                    ? currentNAV - previousNav : 0;
                var dailyChangePct = previousNav > 0
                    ? Math.Round((dailyChange / previousNav) * 100, 4) : 0;

                // This week
                var latestNavDate = navList.Count > 0
                    ? navList[0].NavDate : DateTime.Today;
                var monday = GetThisMonday(latestNavDate);
                var weekRecord = navList
                    .Where(n => n.NavDate.Date <= monday.Date)
                    .OrderByDescending(n => n.NavDate)
                    .FirstOrDefault();

                decimal? weekReturn = null;
                decimal? weekGainAmount = null;

                if (weekRecord != null && weekRecord.NAV > 0 && currentNAV > 0)
                {
                    weekReturn = Math.Round(
                        ((currentNAV - weekRecord.NAV) / weekRecord.NAV) * 100, 4);
                    weekGainAmount = Math.Round(
                        holding.InvestedAmount * weekReturn.Value / 100, 2);
                }

                // Total gain
                var totalGain = currentValue - holding.InvestedAmount;
                var totalGainPct = holding.InvestedAmount > 0
                    ? Math.Round((totalGain / holding.InvestedAmount) * 100, 4) : 0;

                // Avg buy NAV
                var avgBuyNAV = holding.Units > 0
                    ? Math.Round(holding.InvestedAmount / holding.Units, 4) : 0;

                return Result<HoldingSchemeDetailDto>.Success(
                    new HoldingSchemeDetailDto
                    {
                        SchemeCode = holding.SchemeCode,
                        SchemeName = holding.SchemeName,
                        FundName = holding.FundName,
                        FolioNumber = holding.FolioNumber,
                        OrderNumber = holding.Order?.OrderNumber ?? string.Empty,
                        IsApproved = true,

                        InvestedAmount = holding.InvestedAmount,
                        Units = holding.Units,
                        AvgBuyNAV = avgBuyNAV,

                        CurrentNAV = currentNAV,
                        CurrentNavDateText = navList.Count > 0
                            ? navList[0].NavDate.ToString("dd MMM yyyy")
                            : string.Empty,
                        CurrentValue = Math.Round(currentValue, 2),
                        TotalGain = Math.Round(totalGain, 2),
                        TotalGainPercent = totalGainPct,
                        IsTotalGain = totalGain >= 0,

                        PreviousNAV = previousNav,
                        PreviousNavDateText = previousNavDate != DateTime.MinValue
                            ? previousNavDate.ToString("dd MMM yyyy")
                            : string.Empty,
                        DailyChange = Math.Round(dailyChange, 4),
                        DailyChangePercent = dailyChangePct,
                        IsDailyUp = dailyChange >= 0,

                        WeekStartNAV = weekRecord?.NAV,
                        WeekStartDateText = weekRecord?.NavDate
                            .ToString("dd MMM yyyy") ?? string.Empty,
                        WeekReturn = weekReturn,
                        WeekGainAmount = weekGainAmount,
                        IsWeekUp = weekReturn.GetValueOrDefault() >= 0,

                        OneMonth = CalcPeriodDetail("1 Month", 30, navList, currentNAV),
                        ThreeMonth = CalcPeriodDetail("3 Month", 90, navList, currentNAV),
                        SixMonth = CalcPeriodDetail("6 Month", 180, navList, currentNAV),
                        OneYear = CalcPeriodDetail("1 Year", 365, navList, currentNAV),
                        ThreeYear = CalcPeriodDetail("3 Year", 1095, navList, currentNAV)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting scheme detail for {UserId}/{Code}",
                    investorUserId, schemeCode);
                return Result<HoldingSchemeDetailDto>
                    .Failure($"Failed: {ex.Message}");
            }
        }

        // ── Shared helpers ─────────────────────────────────────────
        private async Task<Dictionary<int, (decimal currentNAV, decimal currentValue)>>
            GetLatestSnapshotsAsync(List<int> holdingIds)
        {
            var result = new Dictionary<int,
                (decimal currentNAV, decimal currentValue)>();

            using var conn = new SqlConnection(_connString);

            var sql = @"
                SELECT ps.HoldingId, ps.CurrentNAV, ps.CurrentValue
                FROM PortfolioSnapshots ps
                INNER JOIN (
                    SELECT HoldingId, MAX(SnapshotDate) AS LatestDate
                    FROM PortfolioSnapshots
                    WHERE HoldingId IN @HoldingIds
                    GROUP BY HoldingId
                ) latest
                    ON ps.HoldingId    = latest.HoldingId
                    AND ps.SnapshotDate = latest.LatestDate";

            var rows = await conn.QueryAsync<dynamic>(
                sql, new { HoldingIds = holdingIds });

            foreach (var r in rows)
                result[(int)r.HoldingId] =
                    ((decimal)r.CurrentNAV, (decimal)r.CurrentValue);

            return result;
        }

        private async Task<Dictionary<string, List<NavRecord>>>
            GetNavHistoryAsync(List<string> schemeCodes)
        {
            var result = new Dictionary<string, List<NavRecord>>();
            if (!schemeCodes.Any()) return result;

            using var conn = new SqlConnection(_connString);

            var fromDate = DateTime.Today.AddDays(-1105);

            var sql = @"
                SELECT SchemeCode, NAV, NavDate
                FROM DetailedSchemes
                WHERE SchemeCode IN @SchemeCodes
                  AND NavDate   >= @FromDate
                ORDER BY SchemeCode, NavDate DESC";

            var rows = await conn.QueryAsync<NavRecord>(
                sql, new { SchemeCodes = schemeCodes, FromDate = fromDate });

            foreach (var r in rows)
            {
                if (!result.ContainsKey(r.SchemeCode))
                    result[r.SchemeCode] = new List<NavRecord>();
                result[r.SchemeCode].Add(r);
            }

            return result;
        }

        private QuickReturnDto CalcQuickReturn(
            string label,
            int daysBack,
            List<NavRecord>? navList,
            decimal currentNAV)
        {
            if (navList == null || !navList.Any() || currentNAV <= 0)
                return new QuickReturnDto { Label = label, HasData = false };

            var targetDate = DateTime.Today.AddDays(-daysBack);
            var record = navList
                .Where(n => n.NavDate.Date <= targetDate.Date)
                .OrderByDescending(n => n.NavDate)
                .FirstOrDefault();

            if (record == null || record.NAV <= 0)
                return new QuickReturnDto { Label = label, HasData = false };

            var pct = Math.Round(
                ((currentNAV - record.NAV) / record.NAV) * 100, 4);

            return new QuickReturnDto
            {
                Label = label,
                ReturnPercent = pct,
                IsPositive = pct >= 0,
                HasData = true
            };
        }

        private PeriodDetailDto CalcPeriodDetail(
            string label,
            int daysBack,
            List<NavRecord> navList,
            decimal currentNAV)
        {
            if (!navList.Any() || currentNAV <= 0)
                return new PeriodDetailDto { Label = label, HasData = false };

            var latestDate = navList[0].NavDate;
            var targetDate = latestDate.AddDays(-daysBack);

            var record = navList
                .Where(n => n.NavDate.Date <= targetDate.Date)
                .OrderByDescending(n => n.NavDate)
                .FirstOrDefault();

            if (record == null || record.NAV <= 0)
                return new PeriodDetailDto { Label = label, HasData = false };

            var pct = Math.Round(
                ((currentNAV - record.NAV) / record.NAV) * 100, 4);

            return new PeriodDetailDto
            {
                Label = label,
                ReturnPercent = pct,
                IsPositive = pct >= 0,
                HasData = true
            };
        }

        private static string GetInitials(string name)
        {
            var parts = name.Trim().Split(' ',
                StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                : name.Length >= 2
                    ? name[..2].ToUpper()
                    : name.ToUpper();
        }

        private static string GetCategorySummary(List<Domain.Entities.Holding> holdings)
        {
            // Basic categorisation from scheme name keywords
            var cats = new HashSet<string>();
            foreach (var h in holdings)
            {
                var name = h.SchemeName.ToLower();
                if (name.Contains("equity") || name.Contains("large") ||
                    name.Contains("mid") || name.Contains("small"))
                    cats.Add("Equity");
                else if (name.Contains("debt") || name.Contains("bond") ||
                         name.Contains("liquid") || name.Contains("psu"))
                    cats.Add("Debt");
                else if (name.Contains("hybrid") || name.Contains("balance"))
                    cats.Add("Hybrid");
                else
                    cats.Add("Equity");  // default
            }
            return string.Join(" + ", cats.OrderBy(c => c));
        }

        private static DateTime GetThisMonday(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-diff);
        }

        private class NavRecord
        {
            public string SchemeCode { get; set; } = string.Empty;
            public decimal NAV { get; set; }
            public DateTime NavDate { get; set; }
        }
    }
}