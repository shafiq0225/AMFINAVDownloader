export interface InvestmentOrderDto {
    id: number;
    orderNumber: string;

    // Investor
    investorUserId: string;
    investorName: string;

    // Scheme
    schemeCode: string;
    schemeName: string;
    fundName: string;

    // Amount
    investedAmount: number;

    // Payment
    paymentMode: string;
    chequeNumber?: string;
    chequeDate?: string;
    bankName?: string;
    transactionRef?: string;

    // Dates
    orderDate: string;
    submittedDate?: string;
    confirmedDate?: string;

    // Status
    status: string;
    statusCode: number;

    // Confirmation
    purchaseNAV?: number;
    unitsAllotted?: number;
    folioNumber?: string;

    // Flags
    hasHolding: boolean;
    hasStatement: boolean;

    // Notes + Audit
    notes?: string;
    createdByUserId: string;
    createdAt: string;
    updatedAt: string;
}

export interface CreateOrderDto {
    investorUserId: string;
    investorName: string;
    schemeCode: string;
    schemeName: string;
    fundName: string;
    investedAmount: number;
    paymentMode: string;
    chequeNumber?: string;
    chequeDate?: string;
    bankName?: string;
    transactionRef?: string;
    orderDate: string;
    notes?: string;
}

export interface UpdateOrderStatusDto {
    newStatus: string;
    submittedDate?: string;
    submittedByUserId?: string;
    confirmedDate?: string;
    purchaseNAV?: number;
    unitsAllotted?: number;
    folioNumber?: string;
    notes?: string;
}