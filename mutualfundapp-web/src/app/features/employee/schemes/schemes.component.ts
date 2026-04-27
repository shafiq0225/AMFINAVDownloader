import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { SchemeService } from '../../../core/services/scheme.service';
import { AuthService } from '../../../core/services/auth.service';
import { SchemeEnrollmentDto, UpdateSchemeEnrollmentDto } from '../../../core/models/scheme.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-employee-schemes',
  templateUrl: './schemes.component.html',
  styleUrls: ['./schemes.component.scss'],
  standalone: false
})
export class EmployeeSchemesComponent implements OnInit {
  allSchemes: SchemeEnrollmentDto[] = [];
  filtered: SchemeEnrollmentDto[] = [];
  loading = true;

  searchCtrl = new FormControl('');
  statusFilter = new FormControl('all');

  // Edit inline
  editingCode: string | null = null;
  editingName = new FormControl('');

  canUpdate = false;
  canCreate = false;

  constructor(
    private schemeService: SchemeService,
    public authService: AuthService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.canUpdate = this.authService.hasPermission('scheme.update');
    this.canCreate = this.authService.hasPermission('scheme.create');
    this.loadSchemes();
    this.setupFilters();
  }

  loadSchemes(): void {
    this.loading = true;
    this.schemeService.getAll().subscribe({
      next: (schemes) => {
        this.allSchemes = schemes;
        this.applyFilters();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load schemes.');
        this.cdr.detectChanges();
      }
    });
  }

  setupFilters(): void {
    this.searchCtrl.valueChanges
      .subscribe(() => this.applyFilters());
    this.statusFilter.valueChanges
      .subscribe(() => this.applyFilters());
  }

  applyFilters(): void {
    let result = [...this.allSchemes];
    const search = this.searchCtrl.value?.toLowerCase() || '';
    const status = this.statusFilter.value || 'all';

    if (search) {
      result = result.filter(s =>
        s.schemeCode.toLowerCase().includes(search) ||
        s.schemeName.toLowerCase().includes(search)
      );
    }
    if (status === 'active') result = result.filter(s => s.isApproved);
    if (status === 'inactive') result = result.filter(s => !s.isApproved);

    this.filtered = result;
  }

  clearFilters(): void {
    this.searchCtrl.setValue('');
    this.statusFilter.setValue('all');
  }

  // ── Inline Edit ───────────────────────────────────────────────
  startEdit(scheme: SchemeEnrollmentDto): void {
    if (!this.canUpdate) return;
    this.editingCode = scheme.schemeCode;
    this.editingName.setValue(scheme.schemeName);
  }

  cancelEdit(): void {
    this.editingCode = null;
    this.editingName.setValue('');
  }

  saveEdit(scheme: SchemeEnrollmentDto): void {
    if (!this.editingName.value?.trim()) return;

    const dto: UpdateSchemeEnrollmentDto = {
      schemeName: this.editingName.value.trim(),
      isApproved: scheme.isApproved
    };

    this.schemeService.update(scheme.schemeCode, dto).subscribe({
      next: (updated) => {
        const idx = this.allSchemes.findIndex(
          s => s.schemeCode === updated.schemeCode);
        if (idx !== -1) this.allSchemes[idx] = updated;
        this.applyFilters();
        this.cancelEdit();
        this.toastr.success('Scheme name updated.');
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to update scheme.')
    });
  }

  get totalCount(): number { return this.allSchemes.length; }
  get activeCount(): number { return this.allSchemes.filter(s => s.isApproved).length; }
  get inactiveCount(): number { return this.allSchemes.filter(s => !s.isApproved).length; }
}