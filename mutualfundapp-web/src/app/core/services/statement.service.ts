import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { InvestmentStatementDto } from '../models/investment-statement.model';

@Injectable({ providedIn: 'root' })
export class StatementService {
    private readonly api = `${environment.apiUrl}/api/statements`;

    constructor(private http: HttpClient) { }

    getAll(): Observable<InvestmentStatementDto[]> {
        return this.http.get<InvestmentStatementDto[]>(this.api);
    }

    getByInvestor(userId: string): Observable<InvestmentStatementDto[]> {
        return this.http.get<InvestmentStatementDto[]>(
            `${this.api}/investor/${userId}`);
    }

    getByOrder(orderId: number): Observable<InvestmentStatementDto> {
        return this.http.get<InvestmentStatementDto>(
            `${this.api}/order/${orderId}`);
    }

    getViewUrl(statementId: number): string {
        return `${environment.apiUrl}/api/statements/${statementId}/view`;
    }

    upload(
        orderId: number,
        statementDate: string,
        file: File,
        notes?: string
    ): Observable<InvestmentStatementDto> {
        const form = new FormData();
        form.append('orderId', orderId.toString());
        form.append('statementDate', statementDate);
        form.append('file', file, file.name);
        if (notes) form.append('notes', notes);

        return this.http.post<InvestmentStatementDto>(
            `${this.api}/upload`, form);
    }
}