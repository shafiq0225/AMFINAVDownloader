import {
  Component, Input, OnChanges,
  ViewChild, ElementRef, OnDestroy
} from '@angular/core';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-nav-chart',
  standalone: false,
  template: `
    <div class="chart-wrap">
      <canvas #chartCanvas></canvas>
    </div>
  `,
  styles: [`
    .chart-wrap { position: relative; height: 260px; width: 100%; }
    canvas      { width: 100% !important; }
  `],
})
export class NavChartComponent implements OnChanges, OnDestroy {
  @Input() labels:   string[] = [];
  @Input() datasets: any[]    = [];
  @ViewChild('chartCanvas', { static: true })
  canvasRef!: ElementRef<HTMLCanvasElement>;

  private chart?: Chart;

  ngOnChanges(): void {
    if (!this.labels?.length || !this.datasets?.length) return;
    this.renderChart();
  }

  private renderChart(): void {
    this.chart?.destroy();

    const config: ChartConfiguration = {
      type: 'line',
      data: {
        labels:   this.labels,
        datasets: this.datasets
      },
      options: {
        responsive:          true,
        maintainAspectRatio: false,
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: {
            position: 'top',
            labels: {
              boxWidth:  12,
              boxHeight: 12,
              padding:   16,
              font:      { size: 12, family: 'Inter' }
            }
          },
          tooltip: {
            backgroundColor: '#1F4E79',
            titleFont:  { size: 12, family: 'Inter' },
            bodyFont:   { size: 13, family: 'Inter' },
            padding:    12,
            cornerRadius: 8,
            callbacks: {
              label: (ctx) =>
                ` ${ctx.dataset.label}: ₹${ctx.parsed.y.toFixed(4)}`
            }
          }
        },
        scales: {
          x: {
            grid:      { display: false },
            ticks:     { font: { size: 11, family: 'Inter' } }
          },
          y: {
            grid:      { color: 'rgba(0,0,0,0.04)' },
            ticks: {
              font:     { size: 11, family: 'Inter' },
              callback: (v) => `₹${v}`
            }
          }
        }
      }
    };

    this.chart = new Chart(
      this.canvasRef.nativeElement, config);
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }
}