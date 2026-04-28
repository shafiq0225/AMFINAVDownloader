import {
  Component,
  OnInit,
  Output,
  EventEmitter,
  HostListener,
} from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  roles: string[];
  permission?: string;
  adminOnly?: boolean;
}

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  standalone: false,
})
export class SidebarComponent implements OnInit {
  @Output() collapsedChange = new EventEmitter<boolean>();

  isCollapsed = false;
  isMobileOpen = false;
  activeRoute = '';

  navItems: NavItem[] = [
    {
      label: 'Dashboard',
      icon: 'fa-chart-pie',
      route: '/admin/dashboard',
      roles: ['Admin']
    },
    {
      label: 'Users',
      icon: 'fa-users',
      route: '/admin/users',
      roles: ['Admin']
    },
    {
      label: 'Pending Approvals',
      icon: 'fa-clock',
      route: '/admin/users/pending',
      roles: ['Admin']
    },
    {
      label: 'Permissions',
      icon: 'fa-key',
      route: '/admin/permissions',
      roles: ['Admin']
    },
    {
      label: 'Family Groups',
      icon: 'fa-people-roof',
      route: '/admin/family',
      roles: ['Admin']
    },
    {
      label: 'Schemes',
      icon: 'fa-building-columns',
      route: '/admin/schemes',
      roles: ['Admin']
    },
    {
      label: 'NAV Comparison',
      icon: 'fa-arrow-trend-up',
      route: '/admin/nav',
      roles: ['Admin'],
      adminOnly: true           // ← Admin always sees this
    },

    {
      label: 'Dashboard',
      icon: 'fa-chart-line',
      route: '/employee/dashboard',
      roles: ['Employee']
    },
    {
      label: 'Schemes',
      icon: 'fa-building-columns',
      route: '/employee/schemes',
      roles: ['Employee']
    },
    {
      label: 'NAV Comparison',
      icon: 'fa-arrow-trend-up',
      route: '/employee/nav',
      roles: ['Employee'],
      permission: 'nav.read'    // ← only if Admin granted nav.read
    },

    {
      label: 'Dashboard',
      icon: 'fa-house',
      route: '/user/dashboard',
      roles: ['User']
    },
    {
      label: 'NAV Comparison',
      icon: 'fa-arrow-trend-up',
      route: '/user/nav',
      roles: ['User'],
      permission: 'nav.read'    // ← only if Admin granted nav.read
    },
    {
      label: 'My Family',
      icon: 'fa-people-roof',
      route: '/user/family',
      roles: ['User']
    },
  ];


  constructor(
    public authService: AuthService,
    private router: Router,
  ) { }

  ngOnInit(): void {
    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe((e: any) => (this.activeRoute = e.urlAfterRedirects));
    this.activeRoute = this.router.url;
  }

  get visibleNavItems(): NavItem[] {
    const role = this.authService.userRole;

    return this.navItems.filter(item => {
      // Must match role
      if (!item.roles.includes(role)) return false;

      // Admin always sees adminOnly items
      if (item.adminOnly && role === 'Admin') return true;

      // Permission-gated items — check if user has that permission
      if (item.permission) {
        return this.authService.hasPermission(item.permission);
      }

      return true;
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

  toggleCollapse(): void {
    this.isCollapsed = !this.isCollapsed;
    this.collapsedChange.emit(this.isCollapsed);
  }

  toggleMobile(): void {
    this.isMobileOpen = !this.isMobileOpen;
  }

  closeMobile(): void {
    this.isMobileOpen = false;
  }

  isActive(route: string): boolean {
    return this.activeRoute.startsWith(route);
  }

  navigate(route: string): void {
    this.router.navigate([route]);
    this.closeMobile();
  }

  logout(): void {
    this.authService.logout();
  }

  @HostListener('window:resize')
  onResize(): void {
    if (window.innerWidth > 768) this.isMobileOpen = false;
  }
}
