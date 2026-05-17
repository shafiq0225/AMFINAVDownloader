import { Component, OnInit, ChangeDetectorRef, ViewChild, ElementRef } from '@angular/core';
import { FormControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InvestmentService } from '../../../../core/services/investment.service';
import { SchemeService } from '../../../../core/services/scheme.service';
import { UserService } from '../../../../core/services/user.service';
import { InvestmentOrderDto, CreateOrderDto } from '../../../../core/models/investment-order.model';
import { ToastrService } from 'ngx-toastr';

// ── Scheme group: all orders for one scheme ───────────────────────
export interface OrderSchemeGroup {
  schemeCode: string;
  schemeName: string;
  fundName: string;
  totalInvested: number;
  orderCount: number;
  activeCount: number;
  pendingCount: number;
  orders: InvestmentOrderDto[];
  latestStatus: string;
  latestDate: string;
}

@Component({
  selector: 'app-orders',
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.scss'],
  standalone: false
})
export class OrdersComponent implements OnInit {
  orders: InvestmentOrderDto[] = [];
  loading = true;
  submitting = false;

  searchCtrl = new FormControl('');
  statusFilter = new FormControl('all');

  // Create modal
  showCreateModal = false;
  createForm!: FormGroup;
  isChequePay = false;
  isNeftPay = false;

  // Dropdown data
  users: any[] = [];
  schemes: any[] = [];

  // Tile selection
  selectedSchemeCode: string | null = null;

  @ViewChild('orderDetailPanel') detailPanelRef!: ElementRef;
scheme: any;
tile: any;

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
    this.searchCtrl.valueChanges.subscribe(() => {
      this.selectedSchemeCode = null;
      this.cdr.detectChanges();
    });
    this.statusFilter.valueChanges.subscribe(() => {
      this.selectedSchemeCode = null;
      this.cdr.detectChanges();
    });
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

    this.createForm.get('paymentMode')!.valueChanges.subscribe(mode => {
      this.isChequePay = mode === 'Cheque';
      this.isNeftPay = ['NEFT', 'RTGS', 'IMPS'].includes(mode);
      this.updatePaymentValidators(mode);
    });
    this.isChequePay = true;
  }

  updatePaymentValidators(mode: string): void {
    const cheque = this.createForm.get('chequeNumber')!;
    const date = this.createForm.get('chequeDate')!;
    const bank = this.createForm.get('bankName')!;
    const neft = this.createForm.get('transactionRef')!;

    [cheque, date, bank, neft].forEach(c => {
      c.clearValidators();
      c.updateValueAndValidity();
    });

    if (mode === 'Cheque') {
      cheque.setValidators(Validators.required);
      date.setValidators(Validators.required);
      bank.setValidators(Validators.required);
    } else if (['NEFT', 'RTGS', 'IMPS'].includes(mode)) {
      neft.setValidators(Validators.required);
    }

    [cheque, date, bank, neft].forEach(c => c.updateValueAndValidity());
  }

  loadData(): void {
    this.loading = true;
    this.investmentService.getAll().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load orders.');
      }
    });

    this.userService.getAll().subscribe({
      next: (users) => {
        this.users = users.filter((u: any) => u.statusName === 'Approved');
      }
    });

    this.schemeService.getAll().subscribe({
      next: (schemes) => {
        this.schemes = schemes.filter((s: any) => s.isApproved);
      }
    });
  }

  // ── Filtered orders ───────────────────────────────────────────
  get filteredOrders(): InvestmentOrderDto[] {
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

    return result;
  }

  // ── Scheme groups (tiles) ─────────────────────────────────────
  get groupedSchemes(): OrderSchemeGroup[] {
    const map = new Map<string, OrderSchemeGroup>();

    for (const o of this.filteredOrders) {
      if (!map.has(o.schemeCode)) {
        map.set(o.schemeCode, {
          schemeCode: o.schemeCode,
          schemeName: o.schemeName,
          fundName: o.fundName,
          totalInvested: 0,
          orderCount: 0,
          activeCount: 0,
          pendingCount: 0,
          orders: [],
          latestStatus: o.status,
          latestDate: o.orderDate
        });
      }
      const g = map.get(o.schemeCode)!;
      g.totalInvested += o.investedAmount;
      g.orderCount++;
      if (o.status === 'Active') g.activeCount++;
      if (o.status === 'Pending') g.pendingCount++;
      g.orders.push(o);

      // Track latest order date
      if (new Date(o.orderDate) > new Date(g.latestDate)) {
        g.latestDate = o.orderDate;
        g.latestStatus = o.status;
      }
    }

    return Array.from(map.values())
      .sort((a, b) => a.schemeName.localeCompare(b.schemeName));
  }

  get selectedSchemeGroup(): OrderSchemeGroup | null {
    if (!this.selectedSchemeCode) return null;
    return this.groupedSchemes
      .find(g => g.schemeCode === this.selectedSchemeCode) ?? null;
  }

  selectScheme(schemeCode: string): void {
    const opening = this.selectedSchemeCode !== schemeCode;
    this.selectedSchemeCode = opening ? schemeCode : null;

    if (opening) {
      this.cdr.detectChanges();
      setTimeout(() => this.scrollToPanel(), 50);
    }
  }

  isSchemeSelected(schemeCode: string): boolean {
    return this.selectedSchemeCode === schemeCode;
  }

  private scrollToPanel(): void {
    if (!this.detailPanelRef?.nativeElement) return;
    this.detailPanelRef.nativeElement.scrollIntoView({
      behavior: 'smooth',
      block: 'start'
    });
  }

  // ── Tile helpers ──────────────────────────────────────────────
  getTileStatusClass(group: OrderSchemeGroup): string {
    if (group.activeCount > 0) return 'tile--active';
    if (group.pendingCount > 0) return 'tile--pending';
    return 'tile--other';
  }

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

  // ── Create modal ──────────────────────────────────────────────
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

  onSchemeChange(schemeCode: string): void {
    const scheme = this.schemes.find((s: any) => s.schemeCode === schemeCode);
    if (scheme) {
      this.createForm.patchValue({
        schemeName: scheme.schemeName,
        fundName: scheme.fundName ?? scheme.schemeName.split(' ')[0]
      });
    }
  }

  onInvestorChange(userId: string): void {
    const user = this.users.find((u: any) => u.id === userId);
    if (user) {
      this.createForm.patchValue({ investorName: user.fullName });
    }
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
        this.toastr.success(`Order ${order.orderNumber} created successfully.`);
        this.closeCreateModal();
        this.submitting = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toastr.error(err.error?.error ?? 'Failed to create order.');
        this.submitting = false;
      }
    });
  }

  viewOrder(id: number): void {
    this.router.navigate(['/admin/orders', id]);
  }

  get f() { return this.createForm.controls; }

  get pendingCount(): number { return this.orders.filter(o => o.status === 'Pending').length; }
  get submittedCount(): number { return this.orders.filter(o => o.status === 'Submitted').length; }
  get activeCount(): number { return this.orders.filter(o => o.status === 'Active').length; }
}