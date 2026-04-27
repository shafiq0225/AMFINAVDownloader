import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { NavService } from '../../../core/services/nav.service';
import { FamilyService } from '../../../core/services/family.service';
import { UserService } from '../../../core/services/user.service';
import { NavComparisonResponseDto } from '../../../core/models/nav.model';
import { FamilyGroupDto } from '../../../core/models/family.model';
import { UserDto } from '../../../core/models/user.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-user-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  standalone: false
})
export class UserDashboardComponent implements OnInit {
  loading = true;

  // User info
  currentUser: UserDto | null = null;

  // Data
  navData: NavComparisonResponseDto | null = null;
  familyGroup: FamilyGroupDto | null = null;

  // Permissions
  canReadNav = false;
  canReadFamily = false;

  constructor(
    public authService: AuthService,
    private navService: NavService,
    private familyService: FamilyService,
    private userService: UserService,
    private toastr: ToastrService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.canReadNav = this.authService.hasPermission('nav.read');
    this.canReadFamily = true; // family view always available for User role
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;

    // Always load profile
    const requests: any = {
      profile: this.userService.getMyProfile()
    };

    if (this.canReadNav) {
      requests.nav = this.navService.getDaily();
    }

    forkJoin(requests).subscribe({
      next: (results: any) => {
        this.currentUser = results.profile;
        if (results.nav) {
          this.navData = results.nav;
        }
        this.loading = false;
        this.cdr.detectChanges();

        // Load family group separately after profile
        this.loadFamilyGroup();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load dashboard.');
        this.cdr.detectChanges();
      }
    });
  }

  loadFamilyGroup(): void {
    this.familyService.getAll().subscribe({
      next: (groups) => {
        const userId = this.authService.currentUser?.id;
        // Find group where user is head or member
        this.familyGroup = groups.find(g =>
          g.headUserId === userId ||
          g.members.some(m => m.userId === userId)
        ) ?? null;
        this.cdr.detectChanges();
      },
      error: () => { }
    });
  }

  get userName(): string {
    return this.currentUser?.firstName ?? 'User';
  }

  get userType(): string {
    return this.currentUser?.userTypeName ?? 'None';
  }

  get isHeadOfFamily(): boolean {
    return this.userType === 'HeadOfFamily';
  }

  get isFamilyMember(): boolean {
    return this.userType === 'FamilyMember';
  }

  get navChartLabels(): string[] {
    if (!this.navData?.schemes?.length) return [];
    return this.navData.schemes[0].history.map(h =>
      new Date(h.date).toLocaleDateString('en-IN', {
        day: '2-digit', month: 'short'
      })
    );
  }

  get navChartDatasets(): any[] {
    if (!this.navData?.schemes) return [];
    const colors = ['#1F4E79', '#16A085', '#F39C12'];
    const bgColors = [
      'rgba(31,78,121,0.08)',
      'rgba(22,160,133,0.08)',
      'rgba(243,156,18,0.08)'
    ];
    return this.navData.schemes.slice(0, 3).map((s, i) => ({
      label: s.schemeName.length > 30
        ? s.schemeName.substring(0, 30) + '...'
        : s.schemeName,
      data: s.history.map(h => h.nav),
      borderColor: colors[i],
      backgroundColor: bgColors[i],
      borderWidth: 2,
      fill: true,
      tension: 0.4,
      pointRadius: 4,
      pointHoverRadius: 6
    }));
  }

  goToNav(): void { this.router.navigate(['/user/nav']); }
  goToFamily(): void { this.router.navigate(['/user/family']); }
}