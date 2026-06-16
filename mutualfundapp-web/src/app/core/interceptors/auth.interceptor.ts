import { Injectable } from '@angular/core';
import {
    HttpRequest, HttpHandler, HttpEvent,
    HttpInterceptor, HttpErrorResponse
} from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    private isRefreshing = false;
    private refreshTokenSubject = new BehaviorSubject<string | null>(null);

    constructor(private authService: AuthService) { }

    intercept(
        req: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<any>> {
        if (this.isAuthEndpoint(req.url)) {
            return next.handle(req);
        }

        const token = this.authService.getToken();
        const authReq = token ? this.addToken(req, token) : req;

        return next.handle(authReq).pipe(
            catchError(error => {
                if (error instanceof HttpErrorResponse && error.status === 401) {
                    return this.handle401(req, next);
                }
                return throwError(() => error);
            })
        );
    }

    private handle401(
        req: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<any>> {
        const storedRefreshToken = this.authService.getRefreshToken();

        if (!storedRefreshToken ||
            storedRefreshToken === 'undefined' ||
            storedRefreshToken === 'null') {
            this.authService.logout();
            return throwError(() => new Error('No refresh token'));
        }

        if (!this.isRefreshing) {
            this.isRefreshing = true;
            this.refreshTokenSubject.next(null);

            return this.authService.refresh().pipe(
                switchMap(res => {
                    this.isRefreshing = false;
                    const newToken = res.accessToken ?? (res as any).AccessToken;
                    this.refreshTokenSubject.next(newToken);
                    return next.handle(this.addToken(req, newToken));
                }),
                catchError(err => {
                    this.isRefreshing = false;
                    this.authService.logout();
                    return throwError(() => err);
                })
            );
        }

        return this.refreshTokenSubject.pipe(
            filter(token => token !== null),
            take(1),
            switchMap(token => next.handle(this.addToken(req, token!)))
        );
    }

    private addToken(req: HttpRequest<any>, token: string): HttpRequest<any> {
        return req.clone({
            setHeaders: { Authorization: `Bearer ${token}` }
        });
    }

    private isAuthEndpoint(url: string): boolean {
        return url.includes('/api/auth/login')
            || url.includes('/api/auth/refresh')
            || url.includes('/api/auth/register')
            || url.includes('/api/auth/logout');
    }
}