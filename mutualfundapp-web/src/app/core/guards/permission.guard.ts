import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class PermissionGuard implements CanActivate {
    constructor(
        private authService: AuthService,
        private router: Router,
        private toastr: ToastrService
    ) { }

    canActivate(route: ActivatedRouteSnapshot): boolean {
        const required: string = route.data['permission'] ?? '';

        // Admin bypasses all permission checks
        if (this.authService.userRole === 'Admin') return true;

        if (!required || this.authService.hasPermission(required)) {
            return true;
        }

        this.toastr.error(
            `You need '${required}' permission to access this page.`);
        this.router.navigate([this.getFallback()]);
        return false;
    }

    private getFallback(): string {
        switch (this.authService.userRole) {
            case 'Employee': return '/employee/dashboard';
            default: return '/user/dashboard';
        }
    }
}