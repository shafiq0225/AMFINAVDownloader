export interface InvestmentStatementDto {
    id: number;
    orderId: number;

    orderNumber: string;
    schemeCode: string;
    schemeName: string;
    fundName: string;

    investorUserId: string;
    investorName: string;

    statementDate: string;
    filePath: string;
    fileName: string;
    fileSizeBytes: number;
    fileSizeText: string;

    uploadedByUserId: string;
    uploadedAt: string;
    notes?: string;
}