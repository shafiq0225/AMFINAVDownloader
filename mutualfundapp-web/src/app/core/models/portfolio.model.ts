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