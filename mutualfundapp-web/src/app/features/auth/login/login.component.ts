// src/app/features/auth/login/login.component.ts
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { HolidayService } from '../../../core/services/holiday.service';
import { HolidayStatusDto } from '../../../core/models/HolidayStatusDto';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: false
})
export class LoginComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  showPassword = false;
  errorMessage = '';

  // ── Holiday modal state ────────────────────────────────────────
  showHolidayModal = false;
  holidayStatus: HolidayStatusDto | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private holidayService: HolidayService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    if (this.authService.isLoggedIn) {
      this.redirectByRole();
      return;
    }

    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  get email() { return this.form.get('email')!; }
  get password() { return this.form.get('password')!; }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.login(this.form.value).subscribe({
      next: () => {
        this.loading = false;
        this.checkHolidayThenRedirect();
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage =
          err.error?.message || 'Login failed. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }

  // ── Called when user dismisses the holiday modal ───────────────
  onModalDismissed(): void {
    this.showHolidayModal = false;
    this.redirectByRole();
  }

  // ─────────────────────────────────────────────────────────────
  private checkHolidayThenRedirect(): void {
    this.holidayService.getStatus().subscribe({
      next: (status) => {
        if (status.isHoliday) {
          this.holidayStatus = status;
          this.showHolidayModal = true;
          this.cdr.detectChanges();
          // Redirect happens only after modal is dismissed
        } else {
          this.redirectByRole();
        }
      },
      error: () => {
        // Holiday check failed — don't block the user, just redirect
        this.redirectByRole();
      }
    });
  }

  private redirectByRole(): void {
    const role = this.authService.userRole;
    switch (role) {
      case 'Admin': this.router.navigate(['/admin/dashboard']); break;
      case 'Employee': this.router.navigate(['/employee/dashboard']); break;
      default: this.router.navigate(['/user/dashboard']); break;
    }
  }
}