import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { FamilyService } from '../../../core/services/family.service';
import { FamilyGroupDto } from '../../../core/models/family.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-family-view',
  templateUrl: './family-view.component.html',
  styleUrls: ['./family-view.component.scss'],
  standalone: false
})
export class FamilyViewComponent implements OnInit {
  familyGroup: FamilyGroupDto | null = null;
  loading = true;

  constructor(
    public authService: AuthService,
    private familyService: FamilyService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadFamilyGroup();
  }

  loadFamilyGroup(): void {
    this.loading = true;
    const userId = this.authService.currentUser?.id;

    this.familyService.getAll().subscribe({
      next: (groups) => {
        this.familyGroup = groups.find(g =>
          g.headUserId === userId ||
          g.members.some(m => m.userId === userId)
        ) ?? null;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load family group.');
        this.cdr.detectChanges();
      }
    });
  }

  isCurrentUser(userId: string): boolean {
    return this.authService.currentUser?.id === userId;
  }

  get isHead(): boolean {
    return this.familyGroup?.headUserId
      === this.authService.currentUser?.id;
  }

  getUserInitials(name: string): string {
    const parts = name.split(' ');
    return parts.length >= 2
      ? `${parts[0][0]}${parts[1][0]}`.toUpperCase()
      : name.substring(0, 2).toUpperCase();
  }
}