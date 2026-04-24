import { Injectable } from '@angular/core';
import {
    HttpRequest, HttpHandler, HttpEvent,
    HttpInterceptor, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, catchError } from 'rxjs';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../services/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    constructor(
        private router: Router,
        private toastr: ToastrService,
        private authService: AuthService
    ) { }

    intercept(
        req: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError((error: HttpErrorResponse) => {
                const msg = error.error?.message || 'An error occurred';

                switch (error.status) {
                    case 401:
                        this.authService.logout();
                        this.toastr.warning('Session expired. Please login again.');
                        break;
                    case 403:
                        this.toastr.error('You do not have permission to perform this action.');
                        break;
                    case 404:
                        this.toastr.warning(msg);
                        break;
                    case 409:
                        this.toastr.warning(msg);
                        break;
                    case 400:
                        this.toastr.error(msg);
                        break;
                    case 500:
                        this.toastr.error('Server error. Please try again later.');
                        break;
                    default:
                        this.toastr.error(msg);
                }
                return throwError(() => error);
            })
        );
    }
}