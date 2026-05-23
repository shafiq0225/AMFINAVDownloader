import {
  Component, OnInit, ChangeDetectorRef,
  ViewChild, ElementRef
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PortfolioService } from '../../../../core/services/portfolio.service';
import {
  PortfolioReportDto,
  PortfolioRowDto,
  SchemeGroup,
  buildSchemeGroups
} from '../../../../core/models/portfolio.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-member-portfolio',
  templateUrl: './member-portfolio.component.html',
  styleUrls: ['./member-portfolio.component.scss'],
  standalone: false
})
export class MemberPortfolioComponent implements OnInit {
  portfolio: PortfolioReportDto | null = null;
  loading = true;
  userId = '';

  selectedSchemeCode: string | null = null;

  @ViewChild('detailPanel') detailPanelRef!: ElementRef;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private portfolioService: PortfolioService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    // Re-load when the :userId param changes (sidebar member click)
    this.route.params.subscribe(params => {
      this.userId = params['userId'];
      this.selectedSchemeCode = null;
      this.loadPortfolio();
    });
  }

  loadPortfolio(): void {
    this.loading = true;
    this.portfolioService.getByInvestor(this.userId).subscribe({
      next: data => {
        this.portfolio = data;
        console.log(this.portfolio);
        
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load member portfolio.');
        this.cdr.detectChanges();
      }
    });
  }

  // ── Scheme grouping ───────────────────────────────────────────
  get groupedSchemes(): SchemeGroup[] {
    if (!this.portfolio?.holdings?.length) return [];
    return buildSchemeGroups(this.portfolio.holdings);
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
      this.cdr.detectChanges();
      setTimeout(() => this.scrollToPanel(), 50);
    }
  }

  private scrollToPanel(): void {
    this.detailPanelRef?.nativeElement?.scrollIntoView({
      behavior: 'smooth', block: 'start'
    });
  }

  isSchemeSelected(code: string): boolean {
    return this.selectedSchemeCode === code;
  }

  goBack(): void {
    this.router.navigate(['/admin/portfolio']);
  }

  trackByScheme(_: number, g: SchemeGroup): string { return g.schemeCode; }
}