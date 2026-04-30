import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
    constructor(
        private authService: AuthService,
        private router: Router,
        private toastr: ToastrService
    ) { }

    canActivate(route: ActivatedRouteSnapshot): boolean {
        const allowedRoles: string[] = route.data['roles'] ?? [];
        const userRole = this.authService.userRole;

        if (allowedRoles.length === 0 || allowedRoles.includes(userRole)) {
            return true;
        }

        // ← navigate to /unauthorized instead of showing a toast only
        this.router.navigate(['/unauthorized']);
        return false;
    }
}