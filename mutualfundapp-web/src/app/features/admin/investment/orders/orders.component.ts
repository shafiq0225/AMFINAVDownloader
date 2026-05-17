import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { InvestmentService } from '../../../../core/services/investment.service';
import { SchemeService } from '../../../../core/services/scheme.service';
import { UserService } from '../../../../core/services/user.service';
import { InvestmentOrderDto, CreateOrderDto } from '../../../../core/models/investment-order.model';
import { SchemeEnrollmentDto } from '../../../../core/models/scheme.model';
import { UserDto } from '../../../../core/models/user.model';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-orders',
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.scss'],
  standalone: false
})
export class OrdersComponent implements OnInit {
  orders: InvestmentOrderDto[] = [];
  filtered: InvestmentOrderDto[] = [];
  loading = true;

  // Filters
  searchCtrl = new FormControl('');
  statusFilter = new FormControl('all');

  // Create modal
  showCreateModal = false;
  submitting = false;
  createForm!: FormGroup;

  // Dropdown data
  users: UserDto[] = [];
  schemes: SchemeEnrollmentDto[] = [];

  // Payment mode visibility
  isChequePay = false;
  isNeftPay = false;
  expandedOrders = new Set<number>();

  constructor(
    private investmentService: InvestmentService,
    private schemeService: SchemeService,
    private userService: UserService,
    private fb: FormBuilder,
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.loadData();
    this.setupFilters();
  }

  initForm(): void {
    this.createForm = this.fb.group({
      investorUserId: ['', Validators.required],
      investorName: ['', Validators.required],
      schemeCode: ['', Validators.required],
      schemeName: ['', Validators.required],
      fundName: ['', Validators.required],
      investedAmount: [null, [Validators.required, Validators.min(1)]],
      paymentMode: ['Cheque', Validators.required],
      chequeNumber: [''],
      chequeDate: [''],
      bankName: [''],
      transactionRef: [''],
      orderDate: [new Date().toISOString().split('T')[0], Validators.required],
      notes: ['']
    });

    // Watch payment mode changes
    this.createForm.get('paymentMode')!.valueChanges
      .subscribe(mode => {
        this.isChequePay = mode === 'Cheque';
        this.isNeftPay = ['NEFT', 'RTGS', 'IMPS'].includes(mode);
        this.updatePaymentValidators(mode);
      });

    // Set initial state
    this.isChequePay = true;
  }

  updatePaymentValidators(mode: string): void {
    const chequeCtrl = this.createForm.get('chequeNumber')!;
    const dateCtrl = this.createForm.get('chequeDate')!;
    const bankCtrl = this.createForm.get('bankName')!;
    const neftCtrl = this.createForm.get('transactionRef')!;

    // Reset all
    [chequeCtrl, dateCtrl, bankCtrl, neftCtrl]
      .forEach(c => { c.clearValidators(); c.updateValueAndValidity(); });

    if (mode === 'Cheque') {
      chequeCtrl.setValidators(Validators.required);
      dateCtrl.setValidators(Validators.required);
      bankCtrl.setValidators(Validators.required);
    } else if (['NEFT', 'RTGS', 'IMPS'].includes(mode)) {
      neftCtrl.setValidators(Validators.required);
    }

    [chequeCtrl, dateCtrl, bankCtrl, neftCtrl]
      .forEach(c => c.updateValueAndValidity());
  }

  loadData(): void {
    this.loading = true;

    this.investmentService.getAll().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.applyFilters();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load orders.');
        this.cdr.detectChanges();
      }
    });

    // Load users for investor dropdown
    this.userService.getAll().subscribe({
      next: (users) => {
        this.users = users.filter(u => u.statusName === 'Approved');
      }
    });

    // Load approved schemes for dropdown
    this.schemeService.getAll().subscribe({
      next: (schemes) => {
        this.schemes = schemes.filter(s => s.isApproved);
      }
    });
  }

  setupFilters(): void {
    this.searchCtrl.valueChanges.subscribe(() => this.applyFilters());
    this.statusFilter.valueChanges.subscribe(() => this.applyFilters());
  }

  applyFilters(): void {
    let result = [...this.orders];
    const search = this.searchCtrl.value?.toLowerCase() || '';
    const status = this.statusFilter.value || 'all';

    if (search) {
      result = result.filter(o =>
        o.orderNumber.toLowerCase().includes(search) ||
        o.investorName.toLowerCase().includes(search) ||
        o.schemeName.toLowerCase().includes(search) ||
        o.schemeCode.toLowerCase().includes(search));
    }

    if (status !== 'all') {
      result = result.filter(o =>
        o.status.toLowerCase() === status.toLowerCase());
    }

    this.filtered = result;
  }

  // ── Scheme selection ──────────────────────────────────────────
  onSchemeChange(schemeCode: string): void {
    const scheme = this.schemes.find(s => s.schemeCode === schemeCode);
    if (scheme) {
      this.createForm.patchValue({
        schemeName: scheme.schemeName,
        fundName: scheme.schemeName.split(' ')[0]
      });
    }
  }

  // ── Investor selection ────────────────────────────────────────
  onInvestorChange(userId: string): void {
    const user = this.users.find(u => u.id === userId);
    if (user) {
      this.createForm.patchValue({
        investorName: user.fullName
      });
    }
  }

  // ── Create ────────────────────────────────────────────────────
  openCreateModal(): void {
    this.createForm.reset({
      paymentMode: 'Cheque',
      orderDate: new Date().toISOString().split('T')[0]
    });
    this.isChequePay = true;
    this.isNeftPay = false;
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    const dto: CreateOrderDto = this.createForm.value;

    this.investmentService.create(dto).subscribe({
      next: (order) => {
        this.orders = [order, ...this.orders];
        this.applyFilters();
        this.toastr.success(
          `Order ${order.orderNumber} created successfully.`);
        this.closeCreateModal();
        this.submitting = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toastr.error(
          err.error?.error ?? 'Failed to create order.');
        this.submitting = false;
      }
    });
  }

  viewOrder(id: number): void {
    this.router.navigate(['/admin/orders', id]);
  }

  // ── Helpers ───────────────────────────────────────────────────
  get f() { return this.createForm.controls; }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'badge-warning';
      case 'submitted': return 'badge-info';
      case 'confirmed': return 'badge-accent';
      case 'active': return 'badge-success';
      case 'cancelled': return 'badge-danger';
      default: return 'badge-muted';
    }
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'fa-clock';
      case 'submitted': return 'fa-paper-plane';
      case 'confirmed': return 'fa-circle-check';
      case 'active': return 'fa-circle-play';
      case 'cancelled': return 'fa-circle-xmark';
      default: return 'fa-circle';
    }
  }

  get pendingCount(): number { return this.orders.filter(o => o.status === 'Pending').length; }
  get submittedCount(): number { return this.orders.filter(o => o.status === 'Submitted').length; }
  get activeCount(): number { return this.orders.filter(o => o.status === 'Active').length; }


  toggleOrder(id: number): void {
    if (this.expandedOrders.has(id)) {
      this.expandedOrders.delete(id);
    } else {
      this.expandedOrders.add(id);
    }
  }

  isOrderExpanded(id: number): boolean {
    return this.expandedOrders.has(id);
  }
}