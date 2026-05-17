import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    InvestmentOrderDto,
    CreateOrderDto,
    UpdateOrderStatusDto
} from '../models/investment-order.model';

@Injectable({ providedIn: 'root' })
export class InvestmentService {
    private readonly api = `${environment.apiUrl}/api/orders`;

    constructor(private http: HttpClient) { }

    getAll(status?: string, investorId?: string): Observable<InvestmentOrderDto[]> {
        let params = new HttpParams();
        if (status) params = params.set('status', status);
        if (investorId) params = params.set('investorId', investorId);
        return this.http.get<InvestmentOrderDto[]>(this.api, { params });
    }

    getById(id: number): Observable<InvestmentOrderDto> {
        return this.http.get<InvestmentOrderDto>(`${this.api}/${id}`);
    }

    getByInvestor(userId: string): Observable<InvestmentOrderDto[]> {
        return this.http.get<InvestmentOrderDto[]>(
            `${this.api}/investor/${userId}`);
    }

    create(dto: CreateOrderDto): Observable<InvestmentOrderDto> {
        return this.http.post<InvestmentOrderDto>(this.api, dto);
    }

    updateStatus(
        id: number,
        dto: UpdateOrderStatusDto
    ): Observable<InvestmentOrderDto> {
        return this.http.put<InvestmentOrderDto>(
            `${this.api}/${id}/status`, dto);
    }
}