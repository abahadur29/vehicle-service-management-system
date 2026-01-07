import { ChangeDetectionStrategy, Component, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subscription, timer, take } from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { AuthService } from '../../../../core/auth/services/auth.service';
import { strongPasswordValidator } from '../../../../shared/validators/strong-password.validator';
import { passwordMatchValidator } from '../../../../shared/validators/password-match.validator';
import type { RegisterRequest } from '../../../../shared/models/user.model';

@Component({
  standalone: true,
  selector: 'app-register',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent implements OnDestroy {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  private sub = new Subscription();

  isSubmitting = signal(false);
  serverMessage = signal('');
  registrationSucceeded = signal(false);
  countdown = signal(0);

  form = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.required, Validators.minLength(6)]],
    password: ['', [Validators.required, strongPasswordValidator]],
    confirmPassword: ['', [Validators.required]],
  }, { validators: passwordMatchValidator() });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.serverMessage.set('');

    const dto = this.form.getRawValue();
    const req: RegisterRequest = {
      fullName: dto.fullName ?? '',
      email: dto.email ?? '',
      phoneNumber: dto.phoneNumber ?? '',
      password: dto.password ?? '',
      role: 'Customer',
    };

    this.sub.add(
      this.auth.register(req).subscribe({
        next: (res) => {
          this.isSubmitting.set(false);
          this.registrationSucceeded.set(true);

          this.serverMessage.set('Thank you for joining us! Your account has been created successfully. We are redirecting you to the login page to get started.');

          this.startCountdownRedirect(5);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.registrationSucceeded.set(false);

          const validationErrors = err?.error?.errors;
          if (validationErrors && typeof validationErrors === 'object') {
            this.serverMessage.set(Object.values(validationErrors).flat().join(' '));
            return;
          }

          this.serverMessage.set(
            err?.error?.message ||
            (typeof err?.error === 'string' ? err.error : '') ||
            'Registration failed.'
          );
        },
      })
    );
  }

  private startCountdownRedirect(seconds: number) {
    this.countdown.set(seconds);

    this.sub.add(
      timer(0, 1000)
        .pipe(take(seconds + 1))
        .subscribe((tick) => {
          const remaining = seconds - tick;
          this.countdown.set(Math.max(0, remaining));

          if (remaining <= 0) {
            this.router.navigate(['/auth/login']);
          }
        })
    );
  }

  get pwRules() {
    const value = String(this.form.controls.password.value ?? '');
    return {
      hasUpper: /[A-Z]/.test(value),
      hasLower: /[a-z]/.test(value),
      hasNumber: /[0-9]/.test(value),
      hasSpecial: /[!@#$%^&*()_\-+=\[\]{};:'",.<>/?\\|`~]/.test(value),
      minLength: value.length >= 8,
    };
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }
}