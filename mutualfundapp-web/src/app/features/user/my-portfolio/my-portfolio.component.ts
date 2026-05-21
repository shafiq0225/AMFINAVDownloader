import {
  Component, OnInit, ChangeDetectorRef,
  ViewChild, ElementRef        // ← add these
} from '@angular/core';
import { FormControl } from '@angular/forms';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { AuthService } from '../../../core/services/auth.service';
import { PortfolioReportDto, PortfolioRowDto } from '../../../core/models/portfolio.model';
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
  selector: 'app-my-portfolio',
  templateUrl: './my-portfolio.component.html',
  styleUrls: ['./my-portfolio.component.scss'],
  standalone: false
})
export class MyPortfolioComponent implements OnInit {
  portfolio: PortfolioReportDto | null = null;
  loading = true;
  searchCtrl = new FormControl('');
  selectedSchemeCode: string | null = null;

  // ← Reference to the detail panel DOM element
  @ViewChild('detailPanel') detailPanelRef!: ElementRef;

  constructor(
    private portfolioService: PortfolioService,
    public authService: AuthService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPortfolio();
    this.searchCtrl.valueChanges.subscribe(() => {
      this.selectedSchemeCode = null;
      this.cdr.detectChanges();
    });
  }

  loadPortfolio(): void {
    this.loading = true;
    this.portfolioService.getMyPortfolio().subscribe({
      next: (data) => {
        this.portfolio = data;
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

  get filteredHoldings(): PortfolioRowDto[] {
    if (!this.portfolio) return [];
    const term = this.searchCtrl.value?.toLowerCase() || '';
    if (!term) return this.portfolio.holdings;
    return this.portfolio.holdings.filter(h =>
      h.schemeName.toLowerCase().includes(term) ||
      h.fundName.toLowerCase().includes(term) ||
      h.schemeCode.toLowerCase().includes(term));
  }

  get groupedSchemes(): SchemeGroup[] {
    const map = new Map<string, SchemeGroup>();

    for (const h of this.filteredHoldings) {
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

  get selectedScheme(): SchemeGroup | null {
    if (!this.selectedSchemeCode) return null;
    return this.groupedSchemes
      .find(s => s.schemeCode === this.selectedSchemeCode) ?? null;
  }

  selectScheme(code: string): void {
    const opening = this.selectedSchemeCode !== code;
    this.selectedSchemeCode = opening ? code : null;

    if (opening) {
      // Wait for Angular to render the panel, then scroll to it
      this.cdr.detectChanges();
      setTimeout(() => this.scrollToPanel(), 50);
    }
  }

  private scrollToPanel(): void {
    if (!this.detailPanelRef?.nativeElement) return;

    this.detailPanelRef.nativeElement.scrollIntoView({
      behavior: 'smooth',
      block: 'start'
    });
  }

  isSchemeSelected(code: string): boolean {
    return this.selectedSchemeCode === code;
  }
}