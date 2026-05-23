import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { PortfolioService } from '../../../../core/services/portfolio.service';
import {
  FamilyOverviewDto,
  MemberSummaryDto,
  QuickReturnDto
} from '../../../../core/models/portfolio.model';
import { ToastrService } from 'ngx-toastr';

interface PeriodTab {
  key: string;
  label: string;
}

@Component({
  selector: 'app-portfolio-overview',
  templateUrl: './portfolio-overview.component.html',
  styleUrls: ['./portfolio-overview.component.scss'],
  standalone: false
})
export class PortfolioOverviewComponent implements OnInit {
  overview: FamilyOverviewDto | null = null;
  loading = true;
  snapshotRunning = false;

  // ── Period tabs ───────────────────────────────────────────────
  periods: PeriodTab[] = [
    { key: 'yesterday', label: 'Yesterday' },
    { key: 'd2', label: 'D-2' },
    { key: '1m', label: '1M' },
    { key: '3m', label: '3M' },
    { key: '6m', label: '6M' },
    { key: '1y', label: '1Y' },
    { key: '3y', label: '3Y' },
  ];
  selectedPeriod = 'yesterday';

  constructor(
    private portfolioService: PortfolioService,
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void { this.loadOverview(); }

  loadOverview(): void {
    this.loading = true;
    this.portfolioService.getFamilyOverview().subscribe({
      next: data => {
        this.overview = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load portfolio overview.');
        this.cdr.detectChanges();
      }
    });
  }

  // ── Period helpers ────────────────────────────────────────────
  selectPeriod(key: string): void { this.selectedPeriod = key; }

  getPeriodLabel(): string {
    return this.periods.find(p => p.key === this.selectedPeriod)?.label ?? '';
  }

  isColActive(colKey: string): boolean { return this.selectedPeriod === colKey; }

  /** Returns a member's QuickReturnDto for the currently selected period column. */
  getMemberPeriodReturn(m: MemberSummaryDto): QuickReturnDto | undefined {
    switch (this.selectedPeriod) {
      case 'd2': return m.dayBefore;
      case 'yesterday': return m.yesterday;
      case '1m': return m.oneMonth;
      case '1y': return m.oneYear;
      case '3y': return m.threeYear;
      default: return m.yesterday;
    }
  }

  // ── Format helpers ────────────────────────────────────────────
  formatReturn(r?: QuickReturnDto | null): string {
    if (!r?.hasData) return '—';
    const sign = r.isPositive ? '+' : '';
    return `${sign}${r.returnPercent.toFixed(2)}%`;
  }

  // ── Navigation ────────────────────────────────────────────────
  navigateToMember(userId: string): void {
    this.router.navigate(['/admin/portfolio/member', userId]);
  }

  // ── Snapshot ─────────────────────────────────────────────────
  triggerSnapshot(): void {
    this.snapshotRunning = true;
    this.portfolioService.triggerSnapshot().subscribe({
      next: result => {
        this.snapshotRunning = false;
        this.toastr.success(
          `Snapshot complete — ${result.calculated} holdings calculated.`);
        this.loadOverview();
      },
      error: () => {
        this.snapshotRunning = false;
        this.toastr.error('Snapshot failed.');
      }
    });
  }

  trackByMember(_: number, m: MemberSummaryDto): string {
    return m.investorUserId;
  }
}