import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { StatementService } from '../../../core/services/statement.service';
import { InvestmentStatementDto } from '../../../core/models/investment-statement.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector:    'app-my-statements',
  templateUrl: './my-statements.component.html',
  styleUrls:   ['./my-statements.component.scss'],
  standalone:  false
})
export class MyStatementsComponent implements OnInit {
  statements: InvestmentStatementDto[] = [];
  loading     = true;

  constructor(
    private statementService: StatementService,
    private toastr:           ToastrService,
    private cdr:              ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadStatements();
  }

  loadStatements(): void {
    this.loading = true;
    this.statementService.getAll().subscribe({
      next: (data) => {
        this.statements = data;
        this.loading    = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load statements.');
        this.cdr.detectChanges();
      }
    });
  }

  viewStatement(statement: InvestmentStatementDto): void {
    const url = this.statementService.getViewUrl(statement.id);
    window.open(url, '_blank');
  }
}