import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { PortfolioService } from '../../../../core/services/portfolio.service';
import {
  FamilyOverviewDto,
  MemberSummaryDto,
  QuickReturnDto
} from '../../../../core/models/portfolio.model';
import { ToastrService } from 'ngx-toastr';

interface PeriodTab { key: string; label: string; }

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

  // Mobile accordion state
  expandedMembers = new Set<string>();

  // Period tabs — drive the 3rd column-group in the desktop table
  periods: PeriodTab[] = [
    { key: 'yesterday', label: 'Yesterday' },
    { key: 'd2', label: 'D-2' },
    { key: '1m', label: '1M' },
    { key: '1y', label: '1Y' },
    { key: '3y', label: '3Y' },
    { key: '5y', label: '5Y' },
  ];
  selectedPeriod = '1y';

  // Mobile period rows (all periods, stacked)
  readonly mobilePeriods: PeriodTab[] = [
    { key: 'yesterday', label: 'Yest' },
    { key: '1m', label: '1M' },
    { key: '1y', label: '1Y' },
    { key: '3y', label: '3Y' },
    { key: '5y', label: '5Y' },
  ];

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
        // First member expanded by default on mobile
        if (data.members?.length) {
          this.expandedMembers.add(data.members[0].investorUserId);
        }
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load portfolio overview.');
        this.cdr.detectChanges();
      }
    });
  }

  // ── Period helpers ─────────────────────────────────────────────
  selectPeriod(key: string): void { this.selectedPeriod = key; }

  getPeriodLabel(): string {
    return this.periods.find(p => p.key === this.selectedPeriod)?.label ?? '';
  }

  isColActive(key: string): boolean { return this.selectedPeriod === key; }

  getMemberPeriodReturn(m: MemberSummaryDto): QuickReturnDto | undefined {
    return this.getPeriodReturn(m, this.selectedPeriod);
  }

  getPeriodReturn(m: MemberSummaryDto, key: string): QuickReturnDto | undefined {
    switch (key) {
      case 'd2': return m.dayBefore;
      case 'yesterday': return m.yesterday;
      case '1m': return m.oneMonth;
      case '1y': return m.oneYear;
      case '3y': return m.threeYear;
      case '5y': return m.fiveYear;
      default: return undefined;
    }
  }

  // ── Format helpers ─────────────────────────────────────────────

  /** "+14.30%" or "—" */
  formatReturn(r?: QuickReturnDto | null): string {
    if (!r?.hasData) return '—';
    const sign = r.isPositive ? '+' : '';
    return `${sign}${r.returnPercent.toFixed(2)}%`;
  }

  /** Full rupee amount for desktop: "+₹1,55,200" */
  formatAmountFull(r?: QuickReturnDto | null): string {
    if (!r?.hasData) return '—';
    const val = r.periodGainAmount ?? 0;
    const sign = val >= 0 ? '+' : '−';
    const abs = Math.abs(val);
    return `${sign}₹${abs.toLocaleString('en-IN', { maximumFractionDigits: 0 })}`;
  }

  /** Abbreviated amount for mobile: "+₹1.55L" */
  formatAmount(r?: QuickReturnDto | null): string {
    if (!r?.hasData) return '—';
    const val = r.periodGainAmount ?? 0;
    const sign = val >= 0 ? '+' : '−';
    const abs = Math.abs(val);
    if (abs >= 10_00_000) return `${sign}₹${(abs / 10_00_000).toFixed(2)}Cr`;
    if (abs >= 1_00_000) return `${sign}₹${(abs / 1_00_000).toFixed(2)}L`;
    if (abs >= 1_000) return `${sign}₹${(abs / 1_000).toFixed(1)}K`;
    return `${sign}₹${abs.toFixed(0)}`;
  }

  partialTooltip(r?: QuickReturnDto | null): string {
    if (!r?.isPartialPeriod || !r.actualFromDate) return '';
    return `Data available from ${r.actualFromDate}`;
  }

  // ── Mobile accordion ───────────────────────────────────────────
  toggleMember(userId: string, event: Event): void {
    event.stopPropagation();
    this.expandedMembers.has(userId)
      ? this.expandedMembers.delete(userId)
      : this.expandedMembers.add(userId);
  }

  isMemberExpanded(id: string): boolean { return this.expandedMembers.has(id); }

  // ── Navigation ─────────────────────────────────────────────────
  navigateToMember(userId: string): void {
    this.router.navigate(['/admin/portfolio/member', userId]);
  }

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