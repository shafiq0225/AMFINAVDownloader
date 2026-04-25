import { Component, OnInit } from '@angular/core';
import { UserService } from '../../../core/services/user.service';
import { SchemeService } from '../../../core/services/scheme.service';
import { NavService } from '../../../core/services/nav.service';
import { UserDto } from '../../../core/models/user.model';
import { NavComparisonResponseDto } from '../../../core/models/nav.model';
import { SchemeEnrollmentDto } from '../../../core/models/scheme.model';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  standalone: false
})
export class DashboardComponent implements OnInit {
  loading = true;

  // Stats
  totalUsers = 0;
  pendingUsers = 0;
  activeSchemes = 0;
  navRecords = 0;

  // Data
  pendingList: UserDto[] = [];
  navData: NavComparisonResponseDto | null = null;
  recentSchemes: SchemeEnrollmentDto[] = [];

  constructor(
    private userService: UserService,
    private schemeService: SchemeService,
    private navService: NavService,
    private toastr: ToastrService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;

    forkJoin({
      allUsers: this.userService.getAll(),
      pending: this.userService.getPending(),
      schemes: this.schemeService.getAll(),
      nav: this.navService.getDaily()
    }).subscribe({
      next: ({ allUsers, pending, schemes, nav }) => {
        this.totalUsers = allUsers.length;
        this.pendingUsers = pending.length;
        this.pendingList = pending.slice(0, 5);
        this.activeSchemes = schemes.filter(s => s.isApproved).length;
        this.navRecords = nav.schemes.length;
        this.navData = nav;
        this.recentSchemes = schemes.slice(0, 5);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load dashboard data.');
      }
    });
  }

  approveUser(userId: string): void {
    this.userService.approve(userId).subscribe({
      next: () => {
        this.toastr.success('User approved successfully.');
        this.pendingList = this.pendingList.filter(u => u.id !== userId);
        this.pendingUsers = Math.max(0, this.pendingUsers - 1);
        this.totalUsers++;
      },
      error: () => this.toastr.error('Failed to approve user.')
    });
  }

  rejectUser(userId: string): void {
    this.userService.reject(userId, { reason: 'Rejected by Admin' }).subscribe({
      next: () => {
        this.toastr.warning('User rejected.');
        this.pendingList = this.pendingList.filter(u => u.id !== userId);
        this.pendingUsers = Math.max(0, this.pendingUsers - 1);
      },
      error: () => this.toastr.error('Failed to reject user.')
    });
  }

  get navChartLabels(): string[] {
    if (!this.navData?.schemes?.length) return [];
    return this.navData.schemes[0].history.map(h =>
      new Date(h.date).toLocaleDateString('en-IN', { day: '2-digit', month: 'short' })
    );
  }

  get navChartDatasets(): any[] {
    if (!this.navData?.schemes) return [];
    return this.navData.schemes.slice(0, 3).map((s, i) => ({
      label: s.schemeName.length > 30
        ? s.schemeName.substring(0, 30) + '...'
        : s.schemeName,
      data: s.history.map(h => h.nav),
      borderColor: ['#1F4E79', '#16A085', '#F39C12'][i],
      backgroundColor: ['rgba(31,78,121,0.08)', 'rgba(22,160,133,0.08)', 'rgba(243,156,18,0.08)'][i],
      borderWidth: 2,
      fill: true,
      tension: 0.4,
      pointRadius: 4,
      pointHoverRadius: 6
    }));
  }

  viewAllUsers(): void { this.router.navigate(['/admin/users']); }
  viewPending(): void { this.router.navigate(['/admin/users/pending']); }
  viewSchemes(): void { this.router.navigate(['/admin/permissions']); }
}