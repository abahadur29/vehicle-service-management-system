import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Observable, tap, map } from 'rxjs';

import type { LoginRequest, RegisterRequest, AuthResponse } from '../../../shared/models/user.model';
import type { BackendAuthResponse } from '../../../shared/models/backend-response.model';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private tokenService = inject(TokenService);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/Auth`;

  register(dto: RegisterRequest): Observable<{ message: string }> {
    const payload = {
      FullName: dto.fullName,
      Email: dto.email,
      Password: dto.password,
      PhoneNumber: dto.phoneNumber,
    };

    return this.http.post<{ message: string }>(`${this.baseUrl}/register`, payload);
  }

  login(dto: LoginRequest): Observable<AuthResponse> {
    const payload = {
      Email: dto.email,
      Password: dto.password,
    };

    return this.http.post<BackendAuthResponse>(`${this.baseUrl}/login`, payload).pipe(
      map((res) => {
        return {
          token: res.token ?? res.Token,
          role: res.role ?? res.Role,
          userId: res.userId ?? res.UserId,
          email: res.email ?? res.Email,
          fullName: res.fullName ?? res.FullName,
          isSuccess: res.isSuccess ?? res.IsSuccess,
          message: res.message ?? res.Message,
        } as AuthResponse;
      }),
      tap((normalized) => {
        if (normalized.isSuccess && normalized.token) {
          this.tokenService.setSession({
            token: normalized.token,
            role: normalized.role,
            userId: normalized.userId,
            email: normalized.email,
            fullName: normalized.fullName,
          });
        }
      })
    );
  }

  logout(): void {
    this.tokenService.clear();
  }
}