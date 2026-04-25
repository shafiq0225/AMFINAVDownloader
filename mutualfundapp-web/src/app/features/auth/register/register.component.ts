import { Component, OnInit } from '@angular/core';
import {
  FormBuilder, FormGroup,
  Validators, AbstractControl, ValidationErrors
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

function panValidator(control: AbstractControl): ValidationErrors | null {
  const val = control.value?.toUpperCase() || '';
  return /^[A-Z]{5}[0-9]{4}[A-Z]{1}$/.test(val)
    ? null
    : { invalidPan: true };
}

function passwordMatchValidator(g: AbstractControl): ValidationErrors | null {
  const pw = g.get('password')?.value;
  const cpw = g.get('confirmPassword')?.value;
  return pw === cpw ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  standalone: false
})
export class RegisterComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  showPassword = false;
  showConfirm = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      panNumber: ['', [Validators.required, panValidator]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    }, { validators: passwordMatchValidator });
  }

  get f() { return this.form.controls; }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.loading = true;
    this.errorMessage = '';

    const dto = {
      ...this.form.value,
      panNumber: this.form.value.panNumber?.toUpperCase()
    };

    this.authService.register(dto).subscribe({
      next: (res) => {
        this.loading = false;
        this.successMessage = res.message;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Registration failed.';
      }
    });
  }
}