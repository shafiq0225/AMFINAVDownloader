import { Injectable } from '@angular/core';
import {
    CanActivate, ActivatedRouteSnapshot, Router
} from '@angular/router';
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

        this.toastr.error('Access denied for your role.');
        this.router.navigate([this.getDashboardRoute(userRole)]);
        return false;
    }

    private getDashboardRoute(role: string): string {
        switch (role) {
            case 'Admin': return '/admin/dashboard';
            case 'Employee': return '/employee/dashboard';
            default: return '/user/dashboard';
        }
    }
}