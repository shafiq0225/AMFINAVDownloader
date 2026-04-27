import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { SchemeService } from '../../../core/services/scheme.service';
import { NavService }    from '../../../core/services/nav.service';
import { AuthService }   from '../../../core/services/auth.service';
import { SchemeEnrollmentDto }      from '../../../core/models/scheme.model';
import { NavComparisonResponseDto } from '../../../core/models/nav.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector:    'app-employee-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls:   ['./dashboard.component.scss'],
  standalone:  false
})
export class EmployeeDashboardComponent implements OnInit {
  loading = true;

  // Stats
  totalSchemes  = 0;
  activeSchemes = 0;
  navSchemes    = 0;

  // Data
  recentSchemes: SchemeEnrollmentDto[]        = [];
  navData:       NavComparisonResponseDto | null = null;

  // Permissions
  canReadSchemes  = false;
  canCreateSchemes= false;
  canUpdateSchemes= false;
  canReadNav      = false;
  canApproveFunds = false;

  constructor(
    private schemeService: SchemeService,
    private navService:    NavService,
    public  authService:   AuthService,
    private toastr:        ToastrService,
    private router:        Router,
    private cdr:           ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.checkPermissions();
    this.loadDashboard();
  }
  // Add inside the class:

allPermissions = [
  { code: 'scheme.read',   label: 'Read Schemes' },
  { code: 'scheme.create', label: 'Create Schemes' },
  { code: 'scheme.update', label: 'Update Schemes' },
  { code: 'fund.approval', label: 'Fund Approval' },
  { code: 'nav.read',      label: 'Read NAV Data' },
];

hasPermission(code: string): boolean {
  return this.authService.hasPermission(code);
}
  checkPermissions(): void {
    this.canReadSchemes   = this.authService.hasPermission('scheme.read');
    this.canCreateSchemes = this.authService.hasPermission('scheme.create');
    this.canUpdateSchemes = this.authService.hasPermission('scheme.update');
    this.canReadNav       = this.authService.hasPermission('nav.read');
    this.canApproveFunds  = this.authService.hasPermission('fund.approval');
  }

  loadDashboard(): void {
    this.loading = true;

    const requests: any = {};

    if (this.canReadSchemes) {
      requests.schemes = this.schemeService.getAll();
    }
    if (this.canReadNav) {
      requests.nav = this.navService.getDaily();
    }

    if (Object.keys(requests).length === 0) {
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }

    forkJoin(requests).subscribe({
      next: (results: any) => {
        if (results.schemes) {
          const schemes        = results.schemes as SchemeEnrollmentDto[];
          this.totalSchemes    = schemes.length;
          this.activeSchemes   = schemes.filter(s => s.isApproved).length;
          this.recentSchemes   = schemes.slice(0, 6);
        }
        if (results.nav) {
          const nav        = results.nav as NavComparisonResponseDto;
          this.navData     = nav;
          this.navSchemes  = nav.schemes.length;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load dashboard.');
        this.cdr.detectChanges();
      }
    });
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
    const colors   = ['#1F4E79', '#16A085', '#F39C12'];
    const bgColors = [
      'rgba(31,78,121,0.08)',
      'rgba(22,160,133,0.08)',
      'rgba(243,156,18,0.08)'
    ];
    return this.navData.schemes.slice(0, 3).map((s, i) => ({
      label: s.schemeName.length > 30
        ? s.schemeName.substring(0, 30) + '...'
        : s.schemeName,
      data:             s.history.map(h => h.nav),
      borderColor:      colors[i],
      backgroundColor:  bgColors[i],
      borderWidth:      2,
      fill:             true,
      tension:          0.4,
      pointRadius:      4,
      pointHoverRadius: 6
    }));
  }

  get userName(): string {
    return this.authService.currentUser?.firstName ?? 'Employee';
  }

  get userPermissions(): string[] {
    return this.authService.userPermissions;
  }

  goToSchemes(): void {
    this.router.navigate(['/employee/schemes']);
  }
}