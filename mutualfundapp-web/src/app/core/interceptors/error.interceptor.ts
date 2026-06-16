import { Injectable } from '@angular/core';
import {
    HttpRequest, HttpHandler, HttpEvent,
    HttpInterceptor, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, catchError } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    constructor(private toastr: ToastrService) { }

    intercept(
        req: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError((error: HttpErrorResponse) => {
                const msg = error.error?.message || 'An error occurred';

                switch (error.status) {
                    case 403:
                        this.toastr.error('You do not have permission.');
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
                    // 401 intentionally removed — AuthInterceptor handles it
                }
                return throwError(() => error);
            })
        );
    }
}