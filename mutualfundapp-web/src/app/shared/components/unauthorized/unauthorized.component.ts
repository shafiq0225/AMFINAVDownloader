import { Component } from '@angular/core';
import { Router }    from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:    'app-unauthorized',
  standalone: false,
  templateUrl: './unauthorized.component.html',
  styleUrls:   ['./unauthorized.component.scss']
})
export class UnauthorizedComponent {
  constructor(
    private router:      Router,
    public  authService: AuthService
  ) {}

  goHome(): void {
    if (!this.authService.isLoggedIn) {
      this.router.navigate(['/auth/login']);
      return;
    }
    switch (this.authService.userRole) {
      case 'Admin':    this.router.navigate(['/admin/dashboard']);    break;
      case 'Employee': this.router.navigate(['/employee/dashboard']); break;
      default:         this.router.navigate(['/user/dashboard']);     break;
    }
  }

  goBack(): void {
    window.history.back();
  }

  logout(): void {
    this.authService.logout();
  }

  get userRole(): string {
    return this.authService.userRole || 'User';
  }

  get userName(): string {
    return this.authService.currentUser?.fullName || 'User';
  }
}