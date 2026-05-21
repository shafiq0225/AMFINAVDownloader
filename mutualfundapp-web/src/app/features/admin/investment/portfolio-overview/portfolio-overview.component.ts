import {
  Component, OnInit, ChangeDetectorRef,
  ViewChildren, QueryList, ElementRef   // ← add these
} from '@angular/core';
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
  selectedSchemeKey: string | null = null;

  // ← One ref per investor's detail panel
  @ViewChildren('schemeDetailPanel')
  detailPanels!: QueryList<ElementRef>;

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
      g.holdings.sort((a, b) =>
        new Date(b.purchaseDate).getTime() - new Date(a.purchaseDate).getTime()
      );
    }

    return Array.from(map.values());
  }

  // ── Investor accordion ─────────────────────────────────────────
  toggleInvestor(investorId: string): void {
    this.expandedInvestor =
      this.expandedInvestor === investorId ? null : investorId;
    this.selectedSchemeKey = null;
  }

  isExpanded(investorId: string): boolean {
    return this.expandedInvestor === investorId;
  }

  // ── Scheme tile ────────────────────────────────────────────────
  getSchemesForInvestor(investorId: string): SchemeGroup[] {
    return this.schemeGroupCache.get(investorId) ?? [];
  }

  selectScheme(investorId: string, schemeCode: string): void {
    const key = `${investorId}__${schemeCode}`;
    const opening = this.selectedSchemeKey !== key;

    this.selectedSchemeKey = opening ? key : null;

    if (opening) {
      this.cdr.detectChanges();
      setTimeout(() => this.scrollToPanel(), 50);
    }
  }

  private scrollToPanel(): void {
    // detailPanels is a QueryList — grab the first visible one
    const panel = this.detailPanels?.first;
    if (!panel?.nativeElement) return;

    panel.nativeElement.scrollIntoView({
      behavior: 'smooth',
      block: 'start'
    });
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

  triggerSnapshot(): void {
    this.snapshotRunning = true;
    this.portfolioService.triggerSnapshot().subscribe({
      next: (result) => {
        this.snapshotRunning = false;
        this.toastr.success(
          `Snapshot complete — ${result.calculated} holdings calculated.`);
        this.loadPortfolio();
      },
      error: () => {
        this.snapshotRunning = false;
        this.toastr.error('Snapshot failed.');
      }
    });
  }

  trackByInvestor(_: number, r: PortfolioReportDto): string {
    return r.investorUserId;
  }

  trackByScheme(_: number, g: SchemeGroup): string {
    return g.schemeCode;
  }
}