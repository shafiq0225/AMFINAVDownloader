import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { UserService } from '../../../../core/services/user.service';
import { UserDto } from '../../../../core/models/user.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-pending',
  templateUrl: './pending.component.html',
  styleUrls: ['./pending.component.scss'],
  standalone: false
})
export class PendingComponent implements OnInit {
  pendingUsers: UserDto[] = [];
  loading = true;
  rejectingUser: UserDto | null = null;
  rejectReason = new FormControl('');
  showRejectModal = false;

  constructor(
    private userService: UserService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPending();
  }

  loadPending(): void {
    this.loading = true;
    this.userService.getPending().subscribe({
      next: (users) => {
        this.pendingUsers = users;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load pending users.');
        this.cdr.detectChanges();
      }
    });
  }

  approveUser(user: UserDto): void {
    this.userService.approve(user.id).subscribe({
      next: () => {
        this.pendingUsers = this.pendingUsers.filter(u => u.id !== user.id);
        this.toastr.success(`✅ ${user.fullName} approved successfully.`);
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to approve user.')
    });
  }

  openRejectModal(user: UserDto): void {
    this.rejectingUser = user;
    this.showRejectModal = true;
    this.rejectReason.setValue('');
  }

  closeRejectModal(): void {
    this.showRejectModal = false;
    this.rejectingUser = null;
  }

  confirmReject(): void {
    if (!this.rejectingUser) return;
    const user = this.rejectingUser;
    const reason = this.rejectReason.value || 'Rejected by Admin';

    this.userService.reject(user.id, { reason }).subscribe({
      next: () => {
        this.pendingUsers = this.pendingUsers.filter(u => u.id !== user.id);
        this.showRejectModal = false;
        this.rejectingUser = null;
        this.toastr.warning(`${user.fullName} rejected.`);
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to reject user.')
    });
  }

  approveAll(): void {
    if (!confirm(`Approve all ${this.pendingUsers.length} pending users?`)) return;
    const calls = this.pendingUsers.map(u => this.userService.approve(u.id));
    let completed = 0;

    calls.forEach((call, i) => {
      call.subscribe({
        next: () => {
          completed++;
          if (completed === calls.length) {
            this.pendingUsers = [];
            this.toastr.success(`All ${completed} users approved.`);
            this.cdr.detectChanges();
          }
        }
      });
    });
  }
}