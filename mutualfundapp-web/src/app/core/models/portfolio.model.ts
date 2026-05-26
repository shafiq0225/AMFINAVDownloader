// ─────────────────────────────────────────────────────────────────
// Core row / report DTOs (unchanged)
// ─────────────────────────────────────────────────────────────────

export interface PortfolioRowDto {
    schemeCode: string;
    schemeName: string;
    fundName: string;
    folioNumber: string;
    purchaseDate: string;
    purchaseDateText: string;
    year: number;
    investedAmount: number;
    purchaseNAV: number;
    units: number;
    currentNAV: number;
    totalAmount: number;
    profitLoss: number;
    percentage: number;
    isProfit: boolean;
    snapshotDate?: string;
}

export interface PortfolioReportDto {
    investorUserId: string;
    investorName: string;
    reportDate: string;
    totalInvested: number;
    totalCurrentValue: number;
    totalProfitLoss: number;
    overallReturnPercent: number;
    isOverallProfit: boolean;
    totalHoldings: number;
    holdings: PortfolioRowDto[];
}

export interface FamilyPortfolioDto {
    reportDate: string;
    totalFamilyInvested: number;
    totalFamilyCurrentValue: number;
    totalFamilyProfitLoss: number;
    familyReturnPercent: number;
    isFamilyProfit: boolean;
    investorPortfolios: PortfolioReportDto[];
}

export interface HoldingDto {
    id: number;
    orderId: number;
    orderNumber: string;
    investorUserId: string;
    investorName: string;
    schemeCode: string;
    schemeName: string;
    fundName: string;
    folioNumber: string;
    purchaseDate: string;
    purchaseYear: number;
    purchaseNAV: number;
    investedAmount: number;
    units: number;
    currentNAV: number;
    currentValue: number;
    profitLoss: number;
    profitLossPercent: number;
    isProfit: boolean;
    lastUpdatedDate?: string;
    isActive: boolean;
}

// ─────────────────────────────────────────────────────────────────
// Shared scheme-group model
// ─────────────────────────────────────────────────────────────────

export interface SchemeGroup {
    schemeCode: string;
    schemeName: string;
    fundName: string;
    totalInvested: number;
    totalCurrentValue: number;
    totalProfitLoss: number;
    returnPercent: number;
    isProfit: boolean;
    holdingCount: number;
    holdings: PortfolioRowDto[];
}

export function buildSchemeGroups(holdings: PortfolioRowDto[]): SchemeGroup[] {
    const map = new Map<string, SchemeGroup>();
    for (const h of holdings) {
        if (!map.has(h.schemeCode)) {
            map.set(h.schemeCode, {
                schemeCode: h.schemeCode, schemeName: h.schemeName,
                fundName: h.fundName, totalInvested: 0, totalCurrentValue: 0,
                totalProfitLoss: 0, returnPercent: 0, isProfit: true,
                holdingCount: 0, holdings: []
            });
        }
        const g = map.get(h.schemeCode)!;
        g.totalInvested += h.investedAmount;
        g.totalCurrentValue += h.totalAmount;
        g.totalProfitLoss += h.profitLoss;
        g.holdingCount++;
        g.holdings.push(h);
    }
    for (const g of map.values()) {
        g.returnPercent = g.totalInvested > 0
            ? (g.totalProfitLoss / g.totalInvested) * 100 : 0;
        g.isProfit = g.totalProfitLoss >= 0;
        g.holdings.sort((a, b) =>
            new Date(b.purchaseDate).getTime() - new Date(a.purchaseDate).getTime());
    }
    return Array.from(map.values())
    .sort((a, b) => b.totalInvested - a.totalInvested);
}

// ─────────────────────────────────────────────────────────────────
// Family overview DTOs
// ─────────────────────────────────────────────────────────────────

export interface QuickReturnDto {
    label: string;
    returnPercent: number;
    periodGainAmount: number;   // ← ₹ P&L for the period
    cagrPercent: number;   // ← annualised CAGR (0 for short periods)
    isPositive: boolean;
    hasData: boolean;
    isPartialPeriod: boolean;  // ← true when full history unavailable
    actualFromDate: string;   // ← earliest NAV date used when partial
}

export interface MemberSummaryDto {
    investorUserId: string;
    investorName: string;
    initials: string;
    totalInvested: number;
    totalCurrentValue: number;
    totalGain: number;
    totalGainPercent: number;
    isGain: boolean;
    schemeCount: number;
    holdingCount: number;
    categorySummary: string;
    // period returns
    dayBefore?: QuickReturnDto;
    yesterday?: QuickReturnDto;
    oneMonth?: QuickReturnDto;
    oneYear?: QuickReturnDto;
    threeYear?: QuickReturnDto;
    fiveYear?: QuickReturnDto;   // ← NEW
}

export interface FamilyOverviewDto {
    totalFamilyInvested: number;
    totalFamilyCurrentValue: number;
    totalFamilyGain: number;
    totalFamilyGainPercent: number;
    isFamilyGain: boolean;
    totalMembers: number;
    totalSchemes: number;
    equitySchemeCount: number;
    debtSchemeCount: number;
    hybridSchemeCount: number;
    familyYesterdayReturn?: QuickReturnDto;
    reportDate: string;
    members: MemberSummaryDto[];
}