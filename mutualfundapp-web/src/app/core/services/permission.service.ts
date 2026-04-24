import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    PermissionDto,
    AssignPermissionDto,
    UserPermissionDto
} from '../models/permission.model';

@Injectable({ providedIn: 'root' })
export class PermissionService {
    private readonly api = `${environment.apiUrl}/api/permissions`;

    constructor(private http: HttpClient) { }

    getAll(): Observable<PermissionDto[]> {
        return this.http.get<PermissionDto[]>(this.api);
    }

    getUserPermissions(userId: string): Observable<UserPermissionDto> {
        return this.http.get<UserPermissionDto>(`${this.api}/user/${userId}`);
    }

    assign(dto: AssignPermissionDto): Observable<any> {
        return this.http.post(`${this.api}/assign`, dto);
    }

    revoke(dto: AssignPermissionDto): Observable<any> {
        return this.http.delete(`${this.api}/revoke`, { body: dto });
    }
}