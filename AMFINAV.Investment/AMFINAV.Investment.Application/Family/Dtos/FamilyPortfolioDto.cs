namespace AMFINAV.Investment.Application.Family.Dtos
{
    // ── Screen 1 ───────────────────────────────────────────────────

    public class FamilyOverviewDto
    {
        // Grand total
        public decimal TotalFamilyInvested { get; set; }
        public decimal TotalFamilyCurrentValue { get; set; }
        public decimal TotalFamilyGain { get; set; }
        public decimal TotalFamilyGainPercent { get; set; }
        public bool IsFamilyGain { get; set; }

        public int TotalMembers { get; set; }
        public int TotalSchemes { get; set; }

        public DateTime ReportDate { get; set; }

        // Per-member summary rows
        public IEnumerable<MemberSummaryDto> Members { get; set; }
            = new List<MemberSummaryDto>();
    }

    public class MemberSummaryDto
    {
        public string InvestorUserId { get; set; } = string.Empty;
        public string InvestorName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;

        public decimal TotalInvested { get; set; }
        public decimal TotalCurrentValue { get; set; }
        public decimal TotalGain { get; set; }
        public decimal TotalGainPercent { get; set; }
        public bool IsGain { get; set; }

        public int SchemeCount { get; set; }
        public int HoldingCount { get; set; }

        // Category summary e.g. "Equity + Debt"
        public string CategorySummary { get; set; } = string.Empty;
    }

    // ── Screen 2 ───────────────────────────────────────────────────

    public class MemberHoldingsDto
    {
        public string InvestorUserId { get; set; } = string.Empty;
        public string InvestorName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;

        public decimal TotalInvested { get; set; }
        public decimal TotalCurrentValue { get; set; }
        public decimal TotalGain { get; set; }
        public decimal TotalGainPercent { get; set; }
        public bool IsGain { get; set; }

        public IEnumerable<HoldingCardDto> Holdings { get; set; }
            = new List<HoldingCardDto>();
    }

    public class HoldingCardDto
    {
        public int HoldingId { get; set; }
        public string SchemeCode { get; set; } = string.Empty;
        public string SchemeName { get; set; } = string.Empty;
        public string FundName { get; set; } = string.Empty;
        public string FolioNumber { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;

        // My investment
        public decimal InvestedAmount { get; set; }
        public decimal Units { get; set; }
        public decimal PurchaseNAV { get; set; }
        public decimal CurrentNAV { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Gain { get; set; }
        public decimal GainPercent { get; set; }
        public bool IsGain { get; set; }

        // 4 quick period returns for the mini-grid
        public QuickReturnDto? DayBefore { get; set; }
        public QuickReturnDto? OneMonth { get; set; }
        public QuickReturnDto? SixMonth { get; set; }
        public QuickReturnDto? OneYear { get; set; }
    }

    public class QuickReturnDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal ReturnPercent { get; set; }
        public bool IsPositive { get; set; }
        public bool HasData { get; set; }
    }

    // ── Screen 3 ───────────────────────────────────────────────────

    public class HoldingSchemeDetailDto
    {
        // Identity
        public string SchemeCode { get; set; } = string.Empty;
        public string SchemeName { get; set; } = string.Empty;
        public string FundName { get; set; } = string.Empty;
        public string FolioNumber { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public bool IsApproved { get; set; }

        // My holding
        public decimal InvestedAmount { get; set; }
        public decimal Units { get; set; }
        public decimal AvgBuyNAV { get; set; }
        // InvestedAmount / Units

        // Current value
        public decimal CurrentNAV { get; set; }
        public string CurrentNavDateText { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal TotalGain { get; set; }
        // CurrentValue - InvestedAmount
        public decimal TotalGainPercent { get; set; }
        public bool IsTotalGain { get; set; }

        // Daily NAV compare
        public decimal PreviousNAV { get; set; }
        public string PreviousNavDateText { get; set; } = string.Empty;
        public decimal DailyChange { get; set; }
        public decimal DailyChangePercent { get; set; }
        public bool IsDailyUp { get; set; }

        // This week
        public decimal? WeekStartNAV { get; set; }
        public string WeekStartDateText { get; set; } = string.Empty;
        public decimal? WeekReturn { get; set; }
        public decimal? WeekGainAmount { get; set; }
        // WeekReturn% × InvestedAmount / 100
        public bool IsWeekUp { get; set; }

        // Period returns
        public PeriodDetailDto? OneMonth { get; set; }
        public PeriodDetailDto? ThreeMonth { get; set; }
        public PeriodDetailDto? SixMonth { get; set; }
        public PeriodDetailDto? OneYear { get; set; }
        public PeriodDetailDto? ThreeYear { get; set; }
    }

    public class PeriodDetailDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal ReturnPercent { get; set; }
        public bool IsPositive { get; set; }
        public bool HasData { get; set; }
    }
}