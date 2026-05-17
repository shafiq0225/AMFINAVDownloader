import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { InvestmentService } from '../../../../core/services/investment.service';
import { StatementService } from '../../../../core/services/statement.service';
import { InvestmentOrderDto, UpdateOrderStatusDto } from '../../../../core/models/investment-order.model';
import { InvestmentStatementDto } from '../../../../core/models/investment-statement.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss'],
  standalone: false
})
export class OrderDetailComponent implements OnInit {
  order: InvestmentOrderDto | null = null;
  statement: InvestmentStatementDto | null = null;
  loading = true;
  submitting = false;

  // Status update form
  showStatusModal = false;
  statusForm!: FormGroup;
  nextStatus = '';

  // Statement upload
  showUploadModal = false;
  uploadForm!: FormGroup;
  selectedFile: File | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private investmentService: InvestmentService,
    private statementService: StatementService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.initForms();
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadOrder(id);
  }

  initForms(): void {
    this.statusForm = this.fb.group({
      submittedDate: [new Date().toISOString().split('T')[0]],
      confirmedDate: [new Date().toISOString().split('T')[0]],
      purchaseNAV: [null],
      unitsAllotted: [null],
      folioNumber: [''],
      notes: ['']
    });

    this.uploadForm = this.fb.group({
      statementDate: [new Date().toISOString().split('T')[0],
      Validators.required],
      notes: ['']
    });
  }

  loadOrder(id: number): void {
    this.loading = true;
    this.investmentService.getById(id).subscribe({
      next: (order) => {
        this.order = order;
        this.loading = false;
        this.cdr.detectChanges();

        // Load statement if exists
        if (order.hasStatement) {
          this.statementService.getByOrder(order.id).subscribe({
            next: (s) => {
              this.statement = s;
              this.cdr.detectChanges();
            }
          });
        }
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load order.');
        this.router.navigate(['/admin/orders']);
      }
    });
  }

  // ── Status Transitions ────────────────────────────────────────
  get canSubmit(): boolean { return this.order?.status === 'Pending'; }
  get canConfirm(): boolean { return this.order?.status === 'Submitted'; }
  get canActivate(): boolean { return this.order?.status === 'Confirmed'; }
  get canCancel(): boolean {
    return this.order?.status === 'Pending' ||
      this.order?.status === 'Submitted';
  }

  openStatusModal(status: string): void {
    this.nextStatus = status;
    this.showStatusModal = true;

    // Pre-fill for Confirmed
    if (status === 'Confirmed') {
      this.statusForm.get('purchaseNAV')!
        .setValidators(Validators.required);
      this.statusForm.get('folioNumber')!
        .setValidators(Validators.required);
    } else {
      this.statusForm.get('purchaseNAV')!.clearValidators();
      this.statusForm.get('folioNumber')!.clearValidators();
    }
    this.statusForm.get('purchaseNAV')!.updateValueAndValidity();
    this.statusForm.get('folioNumber')!.updateValueAndValidity();
  }

  closeStatusModal(): void {
    this.showStatusModal = false;
    this.statusForm.reset({
      submittedDate: new Date().toISOString().split('T')[0],
      confirmedDate: new Date().toISOString().split('T')[0]
    });
  }

  submitStatus(): void {
    if (this.statusForm.invalid) {
      this.statusForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    const val = this.statusForm.value;

    const dto: UpdateOrderStatusDto = {
      newStatus: this.nextStatus,
      notes: val.notes
    };

    if (this.nextStatus === 'Submitted') {
      dto.submittedDate = val.submittedDate;
    }
    if (this.nextStatus === 'Confirmed') {
      dto.confirmedDate = val.confirmedDate;
      dto.purchaseNAV = val.purchaseNAV;
      dto.unitsAllotted = val.unitsAllotted || null;
      dto.folioNumber = val.folioNumber;
    }

    this.investmentService
      .updateStatus(this.order!.id, dto)
      .subscribe({
        next: (updated) => {
          this.order = updated;
          this.submitting = false;
          this.closeStatusModal();
          this.toastr.success(
            `Order moved to ${updated.status} successfully.`);
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.toastr.error(
            err.error?.error ?? 'Failed to update status.');
          this.submitting = false;
        }
      });
  }

  // ── Statement Upload ──────────────────────────────────────────
  openUploadModal(): void {
    this.showUploadModal = true;
    this.selectedFile = null;
  }

  closeUploadModal(): void {
    this.showUploadModal = false;
    this.selectedFile = null;
    this.uploadForm.reset({
      statementDate: new Date().toISOString().split('T')[0]
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile = input.files[0];
    }
  }

  submitUpload(): void {
    if (!this.selectedFile || this.uploadForm.invalid) {
      this.uploadForm.markAllAsTouched();
      if (!this.selectedFile)
        this.toastr.warning('Please select a PDF file.');
      return;
    }

    this.submitting = true;
    const val = this.uploadForm.value;

    this.statementService.upload(
      this.order!.id,
      val.statementDate,
      this.selectedFile,
      val.notes
    ).subscribe({
      next: (stmt) => {
        this.statement = stmt;
        if (this.order) this.order.hasStatement = true;
        this.submitting = false;
        this.closeUploadModal();
        this.toastr.success('Statement uploaded successfully.');
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toastr.error(
          err.error?.error ?? 'Failed to upload statement.');
        this.submitting = false;
      }
    });
  }

  viewStatement(): void {
    if (!this.statement) return;
    const url = this.statementService.getViewUrl(this.statement.id);
    window.open(url, '_blank');
  }

  goBack(): void {
    this.router.navigate(['/admin/orders']);
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pending': return 'badge-warning';
      case 'submitted': return 'badge-info';
      case 'confirmed': return 'badge-accent';
      case 'active': return 'badge-success';
      case 'cancelled': return 'badge-danger';
      default: return 'badge-muted';
    }
  }

  get sf() { return this.statusForm.controls; }
  get uf() { return this.uploadForm.controls; }
}