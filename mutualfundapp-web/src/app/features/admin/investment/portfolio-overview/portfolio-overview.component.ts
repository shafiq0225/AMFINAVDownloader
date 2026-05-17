import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { PortfolioService } from '../../../../core/services/portfolio.service';
import {
  FamilyPortfolioDto,
  PortfolioReportDto,
  PortfolioRowDto
} from '../../../../core/models/portfolio.model';
import { ToastrService } from 'ngx-toastr';

export interface SchemeGroup {
  schemeCode: string;
  schemeName: string;
  fundName: string;
  totalInvested: number;
  totalCurrentValue: number;
  totalProfitLoss: number;
  returnPercent: number;
  isProfit: boolean;
  holdingCount: number;
  holdings: PortfolioRowDto[];
}

@Component({
  selector: 'app-portfolio-overview',
  templateUrl: './portfolio-overview.component.html',
  styleUrls: ['./portfolio-overview.component.scss'],
  standalone: false
})
export class PortfolioOverviewComponent implements OnInit {
  familyPortfolio: FamilyPortfolioDto | null = null;
  loading = true;
  snapshotRunning = false;

  expandedInvestor: string | null = null;
  // Format: `${investorUserId}__${schemeCode}` — tracks which tile is open per investor
  selectedSchemeKey: string | null = null;

  private schemeGroupCache = new Map<string, SchemeGroup[]>();

  constructor(
    private portfolioService: PortfolioService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPortfolio();
  }

  loadPortfolio(): void {
    this.loading = true;
    this.portfolioService.getFamilyPortfolio().subscribe({
      next: (data) => {
        this.familyPortfolio = data;
        this.schemeGroupCache.clear();
        for (const inv of data.investorPortfolios) {
          this.schemeGroupCache.set(
            inv.investorUserId,
            this.buildSchemeGroups(inv.holdings)
          );
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load portfolio.');
        this.cdr.detectChanges();
      }
    });
  }

  private buildSchemeGroups(holdings: PortfolioRowDto[]): SchemeGroup[] {
    const map = new Map<string, SchemeGroup>();

    for (const h of holdings) {
      if (!map.has(h.schemeCode)) {
        map.set(h.schemeCode, {
          schemeCode: h.schemeCode,
          schemeName: h.schemeName,
          fundName: h.fundName,
          totalInvested: 0,
          totalCurrentValue: 0,
          totalProfitLoss: 0,
          returnPercent: 0,
          isProfit: true,
          holdingCount: 0,
          holdings: []
        });
      }
      const g = map.get(h.schemeCode)!;
      g.totalInvested += h.investedAmount;
      g.totalCurrentValue += h.totalAmount;
      g.totalProfitLoss += h.profitLoss;
      g.holdingCount++;
      g.holdings.push(h);
    }

    for (const g of map.values()) {
      g.returnPercent = g.totalInvested > 0
        ? (g.totalProfitLoss / g.totalInvested) * 100
        : 0;
      g.isProfit = g.totalProfitLoss >= 0;
    }

    return Array.from(map.values());
  }

  // ── Investor accordion ────────────────────────────────────────
  toggleInvestor(investorId: string): void {
    if (this.expandedInvestor === investorId) {
      this.expandedInvestor = null;
    } else {
      this.expandedInvestor = investorId;
    }
    this.selectedSchemeKey = null;
  }

  isExpanded(investorId: string): boolean {
    return this.expandedInvestor === investorId;
  }

  // ── Scheme tile helpers ───────────────────────────────────────
  getSchemesForInvestor(investorId: string): SchemeGroup[] {
    return this.schemeGroupCache.get(investorId) ?? [];
  }

  selectScheme(investorId: string, schemeCode: string): void {
    const key = `${investorId}__${schemeCode}`;
    this.selectedSchemeKey = this.selectedSchemeKey === key ? null : key;
  }

  isSchemeSelected(investorId: string, schemeCode: string): boolean {
    return this.selectedSchemeKey === `${investorId}__${schemeCode}`;
  }

  getSelectedScheme(investorId: string): SchemeGroup | null {
    if (!this.selectedSchemeKey) return null;
    const [invId, schemeCode] = this.selectedSchemeKey.split('__');
    if (invId !== investorId) return null;
    return this.getSchemesForInvestor(investorId)
      .find(s => s.schemeCode === schemeCode) ?? null;
  }

  // ── Snapshot ──────────────────────────────────────────────────
  triggerSnapshot(): void {
    this.snapshotRunning = true;
    this.portfolioService.triggerSnapshot().subscribe({
      next: (result) => {
        this.snapshotRunning = false;
        this.toastr.success(
          `Snapshot complete — ${result.calculated} holdings calculated.`
        );
        this.loadPortfolio();
      },
      error: () => {
        this.snapshotRunning = false;
        this.toastr.error('Snapshot failed.');
      }
    });
  }

  // ── Track-by helpers ──────────────────────────────────────────
  trackByInvestor(_: number, r: PortfolioReportDto): string {
    return r.investorUserId;
  }

  trackByScheme(_: number, g: SchemeGroup): string {
    return g.schemeCode;
  }
}