import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    PortfolioReportDto,
    FamilyPortfolioDto,
    HoldingDto
} from '../models/portfolio.model';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
    private readonly api = `${environment.apiUrl}/api/portfolio`;

    constructor(private http: HttpClient) { }

    getMyPortfolio(asOfDate?: string): Observable<PortfolioReportDto> {
        let params = new HttpParams();
        if (asOfDate) params = params.set('asOfDate', asOfDate);
        return this.http.get<PortfolioReportDto>(
            `${this.api}/me`, { params });
    }

    getByInvestor(
        userId: string,
        investorName?: string,
        asOfDate?: string
    ): Observable<PortfolioReportDto> {
        let params = new HttpParams();
        if (investorName) params = params.set('investorName', investorName);
        if (asOfDate) params = params.set('asOfDate', asOfDate);
        return this.http.get<PortfolioReportDto>(
            `${this.api}/investor/${userId}`, { params });
    }

    getFamilyPortfolio(asOfDate?: string): Observable<FamilyPortfolioDto> {
        let params = new HttpParams();
        if (asOfDate) params = params.set('asOfDate', asOfDate);
        return this.http.get<FamilyPortfolioDto>(
            `${this.api}/family`, { params });
    }

    getAllHoldings(): Observable<HoldingDto[]> {
        return this.http.get<HoldingDto[]>(`${this.api}/holdings`);
    }

    triggerSnapshot(date?: string): Observable<any> {
        let params = new HttpParams();
        if (date) params = params.set('date', date);
        return this.http.post<any>(
            `${environment.apiUrl}/api/jobs/snapshot`,
            {},
            { params });
    }
}