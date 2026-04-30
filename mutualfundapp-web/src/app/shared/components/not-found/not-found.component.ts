import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-not-found',
  standalone: false,
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.scss',
})
export class NotFoundComponent {
  constructor(
    private router: Router,
    public authService: AuthService
  ) { }

  goHome(): void {
    if (!this.authService.isLoggedIn) {
      this.router.navigate(['/auth/login']);
      return;
    }
    switch (this.authService.userRole) {
      case 'Admin': this.router.navigate(['/admin/dashboard']); break;
      case 'Employee': this.router.navigate(['/employee/dashboard']); break;
      default: this.router.navigate(['/user/dashboard']); break;
    }
  }

  goBack(): void {
    window.history.back();
  }
}