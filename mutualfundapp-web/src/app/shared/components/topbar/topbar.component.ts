import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
  standalone: false
})
export class TopbarComponent implements OnInit {
  @Output() menuToggle = new EventEmitter<void>();

  pageTitle = 'Dashboard';
  showDropdown = false;

  pageTitles: Record<string, string> = {
    '/admin/dashboard': 'Dashboard',
    '/admin/users': 'User Management',
    '/admin/users/pending': 'Pending Approvals',
    '/admin/permissions': 'Permissions',
    '/admin/family': 'Family Groups',
    '/employee/dashboard': 'Dashboard',
    '/employee/schemes': 'Scheme Enrollment',
    '/user/dashboard': 'Dashboard',
    '/user/nav': 'NAV Comparison',
    '/user/family': 'My Family',
  };

  constructor(
    public authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.updateTitle(this.router.url);
    this.router.events.subscribe(() => {
      this.updateTitle(this.router.url);
    });
  }

  get userInitials(): string {
    const u = this.authService.currentUser;
    if (!u) return 'U';
    return `${u.firstName[0]}${u.lastName[0]}`.toUpperCase();
  }

  get userName(): string {
    return this.authService.currentUser?.fullName ?? 'User';
  }

  get userRole(): string {
    return this.authService.userRole;
  }

  get userEmail(): string {
    return this.authService.currentUser?.email ?? '';
  }

  private updateTitle(url: string): void {
    const match = Object.keys(this.pageTitles)
      .find(key => url.startsWith(key));
    this.pageTitle = match ? this.pageTitles[match] : 'AMFINAV';
  }

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }

  closeDropdown(): void {
    this.showDropdown = false;
  }

  logout(): void {
    this.authService.logout();
    this.closeDropdown();
  }
}