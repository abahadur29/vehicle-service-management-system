import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { TokenService } from '../services/token.service';
import { NotificationService } from '../../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const tokenService = inject(TokenService);
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((err) => {
      console.error('HTTP Error Intercepted:', {
        status: err?.status,
        message: err?.message,
        error: err?.error,
        url: err?.url
      });
      
      const isAuthEndpoint = req.url.includes('/api/Auth/login') || req.url.includes('/api/Auth/register');
      
      if (err?.status === 401) {
        const errorMessage = err?.error?.message || '';
        if (errorMessage.includes('token') || errorMessage.includes('expired') || errorMessage.includes('unauthorized')) {
          if (!isAuthEndpoint) {
            console.warn('Unauthorized access (401) - Token may be expired or invalid. Redirecting to login.');
            notificationService.error('Session expired. Please login again.');
            tokenService.clear();
            router.navigate(['/auth/login']);
          } else {
            tokenService.clear();
          }
        } else {
          if (!isAuthEndpoint) {
            const msg = errorMessage || 'Invalid credentials or unauthorized access.';
            notificationService.error(msg);
          }
        }
      } else if (err?.status === 403) {
        if (!isAuthEndpoint) {
          console.warn('Forbidden access (403) - User lacks required role.');
          notificationService.error('You do not have permission to perform this action.');
        }
      } else {
        if (!isAuthEndpoint) {
          const errorMessage = err?.error?.message || err?.message || 'An unexpected error occurred';
          notificationService.error(errorMessage);
        }
      }

      return throwError(() => err);
    })
  );
};