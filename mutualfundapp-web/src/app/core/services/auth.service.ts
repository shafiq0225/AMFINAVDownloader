import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    LoginDto, RegisterDto,
    TokenResponseDto, RegisterResponseDto
} from '../models/auth.model';
import { UserDto } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly api = `${environment.apiUrl}/api/auth`;
    private readonly TOKEN = 'amfinav_token';
    private readonly REFRESH = 'amfinav_refresh';

    private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
    currentUser$ = this.currentUserSubject.asObservable();

    constructor(private http: HttpClient, private router: Router) {
        this.loadUserFromToken();
    }

    get currentUser(): UserDto | null {
        return this.currentUserSubject.value;
    }

    get isLoggedIn(): boolean {
        return !!this.getToken();
    }

    get userRole(): string {
        return this.parseToken()?.role ?? '';
    }

    get userPermissions(): string[] {
        const payload = this.parseToken();
        if (!payload) return [];
        const perms = payload.permissions;
        return Array.isArray(perms) ? perms : perms ? [perms] : [];
    }

    hasPermission(code: string): boolean {
        return this.userRole === 'Admin' ||
            this.userPermissions.includes(code);
    }

    login(dto: LoginDto): Observable<TokenResponseDto> {
        return this.http.post<TokenResponseDto>(`${this.api}/login`, dto)
            .pipe(tap(res => this.storeTokens(res)));
    }

    register(dto: RegisterDto): Observable<RegisterResponseDto> {
        return this.http.post<RegisterResponseDto>(
            `${this.api}/register`, dto);
    }

    refresh(): Observable<TokenResponseDto> {
        const refreshToken = this.getRefreshToken();
        return this.http.post<TokenResponseDto>(
            `${this.api}/refresh`, { refreshToken })
            .pipe(tap(res => this.storeTokens(res)));
    }

    logout(): void {
        const refreshToken = this.getRefreshToken();
        if (refreshToken) {
            this.http.post(`${this.api}/logout`, { refreshToken })
                .subscribe({ error: () => { } });
        }
        this.clearTokens();
        this.currentUserSubject.next(null);
        this.router.navigate(['/auth/login']);
    }

    getToken(): string | null {
        return localStorage.getItem(this.TOKEN);
    }

    getRefreshToken(): string | null {
        return localStorage.getItem(this.REFRESH);
    }

    private storeTokens(res: TokenResponseDto): void {
        localStorage.setItem(this.TOKEN, res.accessToken);
        localStorage.setItem(this.REFRESH, res.refreshToken);
        this.loadUserFromToken();
    }

    private clearTokens(): void {
        localStorage.removeItem(this.TOKEN);
        localStorage.removeItem(this.REFRESH);
    }

    private loadUserFromToken(): void {
        const payload = this.parseToken();
        if (!payload) return;

        const user: UserDto = {
            id: payload.sub,
            firstName: payload.firstName,
            lastName: payload.lastName,
            fullName: `${payload.firstName} ${payload.lastName}`,
            email: payload.email,
            panNumber: payload.panNumber,
            role: 0,
            roleName: payload.role,
            userType: 0,
            userTypeName: payload.userType,
            approvalStatus: 0,
            statusName: payload.approvalStatus,
            isActive: true,
            createdAt: '',
            approvedAt: null,
            lastLoginAt: null,
            rejectionReason: null
        };
        this.currentUserSubject.next(user);
    }

    private parseToken(): any {
        const token = this.getToken();
        if (!token) return null;
        try {
            const payload = token.split('.')[1];
            return JSON.parse(atob(payload));
        } catch {
            return null;
        }
    }
}