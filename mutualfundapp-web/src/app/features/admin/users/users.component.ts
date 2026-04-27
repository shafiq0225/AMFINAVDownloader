import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { UserService } from '../../../core/services/user.service';
import { PermissionService } from '../../../core/services/permission.service';
import { UserDto, UpdateRoleDto } from '../../../core/models/user.model';
import { PermissionDto } from '../../../core/models/permission.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
  standalone: false
})
export class UsersComponent implements OnInit {
  // Data
  allUsers: UserDto[] = [];
  filtered: UserDto[] = [];
  permissions: PermissionDto[] = [];

  // UI state
  loading = true;
  selectedUser: UserDto | null = null;
  showRoleModal = false;
  showPermModal = false;
  userPermCodes: string[] = [];

  // Filters
  searchCtrl = new FormControl('');
  roleFilter = new FormControl('all');
  statusFilter = new FormControl('all');

  // Role modal
  selectedRole = 2;

  constructor(
    private userService: UserService,
    private permissionService: PermissionService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadData();
    this.setupFilters();
  }

  loadData(): void {
    this.loading = true;
    this.userService.getAll().subscribe({
      next: (users) => {
        this.allUsers = users;
        this.applyFilters();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load users.');
        this.cdr.detectChanges();
      }
    });

    this.permissionService.getAll().subscribe({
      next: (perms) => { this.permissions = perms; }
    });
  }

  setupFilters(): void {
    this.searchCtrl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => this.applyFilters());

    this.roleFilter.valueChanges
      .subscribe(() => this.applyFilters());

    this.statusFilter.valueChanges
      .subscribe(() => this.applyFilters());
  }

  applyFilters(): void {
    let result = [...this.allUsers];
    const search = this.searchCtrl.value?.toLowerCase() || '';
    const role = this.roleFilter.value || 'all';
    const status = this.statusFilter.value || 'all';

    if (search) {
      result = result.filter(u =>
        u.fullName.toLowerCase().includes(search) ||
        u.email.toLowerCase().includes(search) ||
        u.panNumber.toLowerCase().includes(search)
      );
    }

    if (role !== 'all') {
      result = result.filter(u => u.roleName === role);
    }

    if (status !== 'all') {
      result = result.filter(u => u.statusName === status);
    }

    this.filtered = result;
  }

  clearFilters(): void {
    this.searchCtrl.setValue('');
    this.roleFilter.setValue('all');
    this.statusFilter.setValue('all');
  }

  // ── Approve ───────────────────────────────────────────────────
  approveUser(user: UserDto): void {
    this.userService.approve(user.id).subscribe({
      next: (updated) => {
        this.updateUserInList(updated);
        this.toastr.success(`${user.fullName} approved.`);
      },
      error: () => this.toastr.error('Failed to approve user.')
    });
  }

  // ── Reject ────────────────────────────────────────────────────
  rejectUser(user: UserDto): void {
    this.userService.reject(user.id, {
      reason: 'Rejected by Admin'
    }).subscribe({
      next: (updated) => {
        this.updateUserInList(updated);
        this.toastr.warning(`${user.fullName} rejected.`);
      },
      error: () => this.toastr.error('Failed to reject user.')
    });
  }

  // ── Role Modal ────────────────────────────────────────────────
  openRoleModal(user: UserDto): void {
    this.selectedUser = user;
    this.selectedRole = user.role;
    this.showRoleModal = true;
  }

  closeRoleModal(): void {
    this.showRoleModal = false;
    this.selectedUser = null;
  }

  saveRole(): void {
    if (!this.selectedUser) return;
    const dto: UpdateRoleDto = { newRole: this.selectedRole };
    this.userService.updateRole(this.selectedUser.id, dto).subscribe({
      next: (updated) => {
        this.updateUserInList(updated);
        this.toastr.success('Role updated successfully.');
        this.closeRoleModal();
      },
      error: () => this.toastr.error('Failed to update role.')
    });
  }

  // ── Permission Modal ──────────────────────────────────────────
  openPermModal(user: UserDto): void {
    this.selectedUser = user;
    this.showPermModal = true;
    this.userPermCodes = [];

    this.permissionService.getUserPermissions(user.id).subscribe({
      next: (res) => {
        this.userPermCodes = res.permissions.map(p => p.code);
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to load permissions.')
    });
  }

  closePermModal(): void {
    this.showPermModal = false;
    this.selectedUser = null;
    this.userPermCodes = [];
  }

  hasPermission(code: string): boolean {
    return this.userPermCodes.includes(code);
  }

  togglePermission(code: string): void {
    if (!this.selectedUser) return;

    if (this.hasPermission(code)) {
      this.permissionService.revoke({
        userId: this.selectedUser.id, permissionCode: code
      }).subscribe({
        next: () => {
          this.userPermCodes = this.userPermCodes.filter(c => c !== code);
          this.toastr.info(`Permission '${code}' revoked.`);
          this.cdr.detectChanges();
        },
        error: () => this.toastr.error('Failed to revoke permission.')
      });
    } else {
      this.permissionService.assign({
        userId: this.selectedUser.id, permissionCode: code
      }).subscribe({
        next: () => {
          this.userPermCodes = [...this.userPermCodes, code];
          this.toastr.success(`Permission '${code}' assigned.`);
          this.cdr.detectChanges();
        },
        error: () => this.toastr.error('Failed to assign permission.')
      });
    }
  }

  // ── Deactivate ────────────────────────────────────────────────
  deactivateUser(user: UserDto): void {
    if (!confirm(`Deactivate ${user.fullName}?`)) return;
    this.userService.reject(user.id, {
      reason: 'Deactivated by Admin'
    }).subscribe({
      next: (updated) => {
        this.updateUserInList(updated);
        this.toastr.warning(`${user.fullName} deactivated.`);
      },
      error: () => this.toastr.error('Failed to deactivate user.')
    });
  }

  private updateUserInList(updated: UserDto): void {
    const idx = this.allUsers.findIndex(u => u.id === updated.id);
    if (idx !== -1) {
      this.allUsers[idx] = updated;
      this.applyFilters();
      this.cdr.detectChanges();
    }
  }

  get totalCount(): number { return this.allUsers.length; }
  get approvedCount(): number { return this.allUsers.filter(u => u.statusName === 'Approved').length; }
  get pendingCount(): number { return this.allUsers.filter(u => u.statusName === 'Pending').length; }
  get rejectedCount(): number { return this.allUsers.filter(u => u.statusName === 'Rejected').length; }

  getRoleBadgeClass(role: string): string {
    switch (role) {
      case 'Admin': return 'badge-primary';
      case 'Employee': return 'badge-accent';
      default: return 'badge-muted';
    }
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Approved': return 'badge-success';
      case 'Pending': return 'badge-warning';
      default: return 'badge-danger';
    }
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'Approved': return 'fa-circle-check';
      case 'Pending': return 'fa-clock';
      default: return 'fa-circle-xmark';
    }
  }
}