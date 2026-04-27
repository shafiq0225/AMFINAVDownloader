import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { FamilyService } from '../../../core/services/family.service';
import { UserService } from '../../../core/services/user.service';
import { FamilyGroupDto, FamilyMemberDto } from '../../../core/models/family.model';
import { UserDto } from '../../../core/models/user.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-family',
  templateUrl: './family.component.html',
  styleUrls: ['./family.component.scss'],
  standalone: false
})
export class FamilyComponent implements OnInit {
  // Data
  groups: FamilyGroupDto[] = [];
  eligibleUsers: UserDto[] = [];
  selectedGroup: FamilyGroupDto | null = null;

  // UI state
  loading = true;
  showCreateModal = false;
  showAddMember = false;
  showDetailPanel = false;
  searchTerm = '';

  // Forms
  createForm!: FormGroup;
  addMemberForm!: FormGroup;

  constructor(
    private familyService: FamilyService,
    private userService: UserService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.initForms();
    this.loadData();
  }

  // Add this getter below initForms()
  get addMemberUserId(): FormControl {
    return this.addMemberForm.get('userId') as FormControl;
  }

  initForms(): void {
    this.createForm = this.fb.group({
      groupName: ['', [Validators.required, Validators.minLength(3)]],
      headUserId: ['', Validators.required]
    });

    this.addMemberForm = this.fb.group({
      userId: ['', Validators.required]
    });
  }

  loadData(): void {
    this.loading = true;

    this.familyService.getAll().subscribe({
      next: (groups) => {
        this.groups = groups;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load family groups.');
        this.cdr.detectChanges();
      }
    });

    // Load eligible users (Role = User, not already in a group)
    this.userService.getAll().subscribe({
      next: (users) => {
        this.eligibleUsers = users.filter(
          u => u.roleName === 'User' && u.statusName === 'Approved'
        );
        this.cdr.detectChanges();
      }
    });
  }

  // ── Filtered groups ───────────────────────────────────────────
  get filteredGroups(): FamilyGroupDto[] {
    if (!this.searchTerm) return this.groups;
    const term = this.searchTerm.toLowerCase();
    return this.groups.filter(g =>
      g.groupName.toLowerCase().includes(term) ||
      g.headUserName.toLowerCase().includes(term) ||
      g.headPanNumber.toLowerCase().includes(term)
    );
  }

  // ── Create Group ──────────────────────────────────────────────
  openCreateModal(): void {
    this.createForm.reset();
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
    this.familyService.create(this.createForm.value).subscribe({
      next: (group) => {
        this.groups = [...this.groups, group];
        this.toastr.success(`Family group '${group.groupName}' created.`);
        this.closeCreateModal();
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to create family group.')
    });
  }

  // ── View Group Detail ─────────────────────────────────────────
  openDetail(group: FamilyGroupDto): void {
    this.selectedGroup = group;
    this.showDetailPanel = true;
    this.addMemberForm.reset();
    this.showAddMember = false;
  }

  closeDetail(): void {
    this.showDetailPanel = false;
    this.selectedGroup = null;
  }

  // ── Add Member ────────────────────────────────────────────────
  toggleAddMember(): void {
    this.showAddMember = !this.showAddMember;
    if (!this.showAddMember) this.addMemberForm.reset();
  }

  submitAddMember(): void {
    if (!this.selectedGroup || this.addMemberForm.invalid) {
      this.addMemberForm.markAllAsTouched();
      return;
    }

    const groupId = this.selectedGroup.id;
    const dto = { userId: this.addMemberForm.value.userId };

    this.familyService.addMember(groupId, dto).subscribe({
      next: (updated) => {
        // Update group in list and detail panel
        this.selectedGroup = updated;
        const idx = this.groups.findIndex(g => g.id === groupId);
        if (idx !== -1) this.groups[idx] = updated;

        this.toastr.success('Member added successfully.');
        this.addMemberForm.reset();
        this.showAddMember = false;
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to add member.')
    });
  }

  // ── Remove Member ─────────────────────────────────────────────
  removeMember(member: FamilyMemberDto): void {
    if (!this.selectedGroup) return;
    if (!confirm(`Remove ${member.fullName} from this group?`)) return;

    const groupId = this.selectedGroup.id;

    this.familyService.removeMember(groupId, member.userId).subscribe({
      next: () => {
        if (this.selectedGroup) {
          this.selectedGroup = {
            ...this.selectedGroup,
            members: this.selectedGroup.members.filter(
              m => m.userId !== member.userId
            )
          };
          const idx = this.groups.findIndex(g => g.id === groupId);
          if (idx !== -1) this.groups[idx] = this.selectedGroup;
        }
        this.toastr.warning(`${member.fullName} removed from group.`);
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to remove member.')
    });
  }

  // ── Helpers ───────────────────────────────────────────────────
  get availableUsersForGroup(): UserDto[] {
    if (!this.selectedGroup) return this.eligibleUsers;

    const memberIds = new Set(
      this.selectedGroup.members.map(m => m.userId)
    );
    memberIds.add(this.selectedGroup.headUserId);

    return this.eligibleUsers.filter(u => !memberIds.has(u.id));
  }

  getUserInitials(name: string): string {
    const parts = name.split(' ');
    return parts.length >= 2
      ? `${parts[0][0]}${parts[1][0]}`.toUpperCase()
      : name.substring(0, 2).toUpperCase();
  }

  get f() { return this.createForm.controls; }
  get mf() { return this.addMemberForm.controls; }
}