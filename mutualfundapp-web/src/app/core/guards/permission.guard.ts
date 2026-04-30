import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class PermissionGuard implements CanActivate {
    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    canActivate(route: ActivatedRouteSnapshot): boolean {
        const required: string = route.data['permission'] ?? '';

        if (this.authService.userRole === 'Admin') return true;

        if (!required || this.authService.hasPermission(required)) {
            return true;
        }

        // ← navigate to /unauthorized
        this.router.navigate(['/unauthorized']);
        return false;
    }
}