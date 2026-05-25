import { Component, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
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
  viewMode: 'cards' | 'compact' = 'cards';
  searchTerm = '';
  sortBy = 'name';
  expandedMember: string | null = null;

  // Period tabs
  periods: PeriodTab[] = [
    { key: 'yesterday', label: 'Yesterday' },
    { key: 'd2', label: 'D-2' },
    { key: '1m', label: '1M' },
    { key: '1y', label: '1Y' },
    { key: '3y', label: '3Y' },
    { key: '5y', label: '5Y' },
  ];
  selectedPeriod = '1y';

  // Mobile period rows
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

  // ── View mode ──────────────────────────────────────────────────
  toggleView(): void {
    this.viewMode = this.viewMode === 'cards' ? 'compact' : 'cards';
  }

  // ── Search & Filter ────────────────────────────────────────────
  getFilteredMembers(): MemberSummaryDto[] {
    if (!this.overview?.members) return [];
    let members = this.overview.members;

    // Filter
    if (this.searchTerm) {
      members = members.filter(m =>
        m.investorName.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    }

    // Sort
    switch (this.sortBy) {
      case 'name':
        members = members.sort((a, b) => a.investorName.localeCompare(b.investorName));
        break;
      case 'value':
        members = members.sort((a, b) => b.totalCurrentValue - a.totalCurrentValue);
        break;
      case 'return':
        members = members.sort((a, b) => b.totalGainPercent - a.totalGainPercent);
        break;
      case 'invested':
        members = members.sort((a, b) => b.totalInvested - a.totalInvested);
        break;
    }

    return members;
  }

  // ── Performance helpers ────────────────────────────────────────
  isTopPerformer(member: MemberSummaryDto): boolean {
    if (!this.overview?.members?.length) return false;
    const sorted = [...this.overview.members].sort((a, b) => b.totalGainPercent - a.totalGainPercent);
    return sorted[0]?.investorUserId === member.investorUserId;
  }

  isBottomPerformer(member: MemberSummaryDto): boolean {
    if (!this.overview?.members?.length) return false;
    const sorted = [...this.overview.members].sort((a, b) => a.totalGainPercent - b.totalGainPercent);
    return sorted[0]?.investorUserId === member.investorUserId;
  }

  getReturnHistory(member: MemberSummaryDto): number[] {
    // Generate mock return history for sparkline
    // In real app, this would come from the API
    const history = [];
    for (let i = 0; i < 10; i++) {
      history.push(Math.random() * 20 - 10);
    }
    return history;
  }

  // ── Footer stats ──────────────────────────────────────────────
  getBestPerformer(): string {
    if (!this.overview?.members?.length) return '—';
    const best = this.overview.members.reduce((a, b) =>
      a.totalGainPercent > b.totalGainPercent ? a : b
    );
    return `${best.investorName} (${best.totalGainPercent.toFixed(2)}%)`;
  }

  getWorstPerformer(): string {
    if (!this.overview?.members?.length) return '—';
    const worst = this.overview.members.reduce((a, b) =>
      a.totalGainPercent < b.totalGainPercent ? a : b
    );
    return `${worst.investorName} (${worst.totalGainPercent.toFixed(2)}%)`;
  }

  getAverageReturn(): number {
    if (!this.overview?.members?.length) return 0;
    const sum = this.overview.members.reduce((total, m) => total + m.totalGainPercent, 0);
    return sum / this.overview.members.length;
  }

  // ── Format helpers ─────────────────────────────────────────────
  formatReturn(r?: QuickReturnDto | null): string {
    if (!r?.hasData) return '—';
    const sign = r.isPositive ? '+' : '';
    return `${sign}${r.returnPercent.toFixed(2)}%`;
  }

  formatAmountFull(r?: QuickReturnDto | null): string {
    if (!r?.hasData) return '—';
    const val = r.periodGainAmount ?? 0;
    const sign = val >= 0 ? '+' : '−';
    const abs = Math.abs(val);
    return `${sign}₹${abs.toLocaleString('en-IN', { maximumFractionDigits: 0 })}`;
  }

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

  getPercentageWidth(percent: number): number {
    return Math.min(Math.abs(percent), 100);
  }

  // ── Navigation ─────────────────────────────────────────────────
  navigateToMember(userId: string): void {
    this.router.navigate(['/admin/portfolio/member', userId]);
  }

  // ── Snapshot ────────────────────────────────────────────────────
  triggerSnapshot(): void {
    this.snapshotRunning = true;
    this.portfolioService.triggerSnapshot().subscribe({
      next: result => {
        this.snapshotRunning = false;
        this.toastr.success(`Snapshot complete — ${result.calculated} holdings calculated.`);
        this.loadOverview();
      },
      error: () => {
        this.snapshotRunning = false;
        this.toastr.error('Snapshot failed.');
      }
    });
  }

  // ── Keyboard shortcuts ──────────────────────────────────────────
  @HostListener('document:keydown', ['$event'])
  handleKeyboard(event: KeyboardEvent): void {
    // 'R' for refresh
    if (event.key === 'r' || event.key === 'R') {
      if (!this.snapshotRunning) {
        this.triggerSnapshot();
      }
    }
    // 'V' for view toggle
    if (event.key === 'v' || event.key === 'V') {
      this.toggleView();
    }
    // Number keys 1-6 for periods
    const periodKeys = ['yesterday', 'd2', '1m', '1y', '3y', '5y'];
    const num = parseInt(event.key);
    if (num >= 1 && num <= 6) {
      this.selectPeriod(periodKeys[num - 1]);
    }
  }

  trackByMember(_: number, m: MemberSummaryDto): string {
    return m.investorUserId;
  }
}