import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SchemeDetailsDto, PeriodReturnDto } from '../../../core/models/scheme-details.model';
import { SchemeDetailsService } from '../../../core/services/scheme-details.service';

@Component({
  selector: 'app-scheme-details',
  standalone: false,
  templateUrl: './scheme-details.component.html',
  styleUrl: './scheme-details.component.scss',
})
export class SchemeDetailsComponent implements OnInit, AfterViewInit {
scheme: SchemeDetailsDto | null = null;
  loading = true;
  schemeCode = '';

  @ViewChild('sparklineCanvas')
  canvasRef!: ElementRef<HTMLCanvasElement>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: SchemeDetailsService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.schemeCode = this.route.snapshot.paramMap.get('schemeCode') || '';
    this.loadDetails();
  }

  ngAfterViewInit(): void {
    // Draw chart after view init if data is already loaded
    if (this.scheme?.navHistory?.length) {
      this.drawSparkline();
    }
  }

  loadDetails(): void {
    this.loading = true;
    this.service.getSchemeDetails(this.schemeCode).subscribe({
      next: (data) => {
        this.scheme = data;
        this.loading = false;
        this.cdr.detectChanges();
        // Small timeout to ensure canvas is rendered
        setTimeout(() => this.drawSparkline(), 100);
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load scheme details.');
        this.cdr.detectChanges();
      }
    });
  }

  // ── Sparkline chart ─────────────────────────────────────────
  drawSparkline(): void {
    const canvas = this.canvasRef?.nativeElement;
    if (!canvas || !this.scheme?.navHistory?.length) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const data = this.scheme.navHistory;
    const W = canvas.offsetWidth || 350;
    const H = canvas.offsetHeight || 80;

    canvas.width = W;
    canvas.height = H;

    const navs = data.map(d => d.nav);
    const minNav = Math.min(...navs);
    const maxNav = Math.max(...navs);
    const range = maxNav - minNav || 1;

    const pad = { top: 8, right: 8, bottom: 8, left: 8 };
    const chartW = W - pad.left - pad.right;
    const chartH = H - pad.top - pad.bottom;

    const xStep = chartW / (data.length - 1);

    const points = data.map((d, i) => ({
      x: pad.left + i * xStep,
      y: pad.top + chartH - ((d.nav - minNav) / range) * chartH
    }));

    // Gradient fill
    const isUp = this.scheme.isDailyUp;
    const color = isUp ? '#00D4A0' : '#FF6B6B';
    const grad = ctx.createLinearGradient(0, 0, 0, H);
    grad.addColorStop(0, isUp
      ? 'rgba(0,212,160,0.25)'
      : 'rgba(255,107,107,0.25)');
    grad.addColorStop(1, 'rgba(0,0,0,0)');

    // Draw fill
    ctx.beginPath();
    ctx.moveTo(points[0].x, points[0].y);
    points.forEach(p => ctx.lineTo(p.x, p.y));
    ctx.lineTo(points[points.length - 1].x, H);
    ctx.lineTo(points[0].x, H);
    ctx.closePath();
    ctx.fillStyle = grad;
    ctx.fill();

    // Draw line
    ctx.beginPath();
    ctx.moveTo(points[0].x, points[0].y);
    points.forEach(p => ctx.lineTo(p.x, p.y));
    ctx.strokeStyle = color;
    ctx.lineWidth = 2;
    ctx.lineJoin = 'round';
    ctx.stroke();

    // Endpoint dot
    const last = points[points.length - 1];
    ctx.beginPath();
    ctx.arc(last.x, last.y, 4, 0, Math.PI * 2);
    ctx.fillStyle = color;
    ctx.fill();
  }

  // ── Period returns as array ──────────────────────────────────
  get periodReturns(): PeriodReturnDto[] {
    if (!this.scheme) return [];
    return [
      this.scheme.oneMonth,
      this.scheme.threeMonth,
      this.scheme.sixMonth,
      this.scheme.oneYear,
      this.scheme.threeYear
    ].filter((p): p is PeriodReturnDto => !!p && p.hasData);
  }

  // ── Bar width for period return (0–100) ─────────────────────
  getBarWidth(returnPct: number): number {
    const abs = Math.abs(returnPct);
    const max = Math.max(
      ...this.periodReturns.map(p => Math.abs(p.returnPercent)),
      1
    );
    return Math.min(Math.round((abs / max) * 100), 100);
  }

  goBack(): void {
    this.router.navigate(['../..'], { relativeTo: this.route });
  }
}
