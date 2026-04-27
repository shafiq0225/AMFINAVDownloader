import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { NavService }  from '../../../core/services/nav.service';
import { AuthService } from '../../../core/services/auth.service';
import { NavComparisonResponseDto, SchemeComparisonDto } from '../../../core/models/nav.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector:    'app-nav-view',
  templateUrl: './nav-view.component.html',
  styleUrls:   ['./nav-view.component.scss'],
  standalone:  false
})
export class NavViewComponent implements OnInit {
  navData:  NavComparisonResponseDto | null = null;
  filtered: SchemeComparisonDto[]           = [];
  loading   = true;

  searchCtrl   = new FormControl('');
  viewMode     = new FormControl('chart');  // chart | table

  // Date range
  startDate = new FormControl('');
  endDate   = new FormControl('');

  canReadNav = false;

  constructor(
    private navService:  NavService,
    public  authService: AuthService,
    private toastr:      ToastrService,
    private cdr:         ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.canReadNav = this.authService.hasPermission('nav.read');
    if (this.canReadNav) {
      this.loadDaily();
      this.searchCtrl.valueChanges.subscribe(() => this.applyFilter());
    } else {
      this.loading = false;
    }
  }

  loadDaily(): void {
    this.loading = true;
    this.navService.getDaily().subscribe({
      next: (data) => {
        this.navData = data;
        this.filtered = data.schemes;
        this.loading  = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load NAV data.');
        this.cdr.detectChanges();
      }
    });
  }

  loadByDateRange(): void {
    const start = this.startDate.value;
    const end   = this.endDate.value;

    if (!start || !end) {
      this.toastr.warning('Please select both start and end dates.');
      return;
    }
    if (new Date(start) >= new Date(end)) {
      this.toastr.warning('Start date must be before end date.');
      return;
    }

    this.loading = true;
    this.navService.getByDateRange(start, end).subscribe({
      next: (data) => {
        this.navData  = data;
        this.filtered = data.schemes;
        this.loading  = false;
        this.applyFilter();
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('No NAV data found for the selected range.');
        this.cdr.detectChanges();
      }
    });
  }

  applyFilter(): void {
    if (!this.navData) return;
    const term = this.searchCtrl.value?.toLowerCase() || '';
    this.filtered = term
      ? this.navData.schemes.filter(s =>
          s.schemeName.toLowerCase().includes(term) ||
          s.schemeCode.toLowerCase().includes(term) ||
          s.fundName.toLowerCase().includes(term))
      : this.navData.schemes;
  }

  getChartLabels(scheme: SchemeComparisonDto): string[] {
    return scheme.history.map(h =>
      new Date(h.date).toLocaleDateString('en-IN', {
        day: '2-digit', month: 'short'
      })
    );
  }

  getChartDatasets(scheme: SchemeComparisonDto): any[] {
    return [{
      label:           scheme.schemeName.length > 30
        ? scheme.schemeName.substring(0, 30) + '...'
        : scheme.schemeName,
      data:            scheme.history.map(h => h.nav),
      borderColor:     '#1F4E79',
      backgroundColor: 'rgba(31,78,121,0.08)',
      borderWidth:     2,
      fill:            true,
      tension:         0.4,
      pointRadius:     5,
      pointHoverRadius:7
    }];
  }

  getLatestNav(scheme: SchemeComparisonDto): number {
    return scheme.history[scheme.history.length - 1]?.nav ?? 0;
  }

  getLatestPct(scheme: SchemeComparisonDto): string {
    return scheme.history[scheme.history.length - 1]?.percentage ?? '0';
  }

  isGrowth(scheme: SchemeComparisonDto): boolean {
    return scheme.history[scheme.history.length - 1]?.isGrowth ?? false;
  }
}