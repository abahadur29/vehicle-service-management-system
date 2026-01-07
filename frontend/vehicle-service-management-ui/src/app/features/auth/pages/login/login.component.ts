import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { AuthService } from '../../../../core/auth/services/auth.service';
import { TokenService } from '../../../../core/auth/services/token.service';
import type { LoginRequest } from '../../../../shared/models/user.model';

@Component({
  standalone: true,
  selector: 'app-login',
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
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private token = inject(TokenService);
  private router = inject(Router);

  isSubmitting = signal(false);
  serverMessage = signal('');

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.serverMessage.set('');

    const raw = this.form.getRawValue();
    const req: LoginRequest = {
      email: raw.email ?? '',
      password: raw.password ?? '',
    };

    this.auth
      .login(req)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (res) => {
          if (!res?.isSuccess) {
            this.serverMessage.set(res?.message || 'Login failed.');
            return;
          }

          // Send admins and managers to admin dashboard, others to regular dashboard
          const role = this.token.getRole()?.toLowerCase();
          if (role === 'admin' || role === 'manager') {
            this.router.navigate(['/admin/dashboard']);
          } else {
            this.router.navigate(['/dashboard']);
          }
        },
        error: (err) => {
          // Show the actual error from backend
          this.serverMessage.set(
            err?.error?.message ||
            (typeof err?.error === 'string' ? err.error : '') ||
            'Login request failed (check backend + https certificate).'
          );
        },
      });
  }
}