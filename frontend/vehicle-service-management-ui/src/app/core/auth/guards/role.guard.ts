import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';

export function roleGuard(allowedRoles: string[]): CanActivateFn {
  return () => {
    const tokenService = inject(TokenService);
    const router = inject(Router);

    const role = tokenService.getRole();

    if (!tokenService.isLoggedIn()) {
      router.navigate(['/auth/login']);
      return false;
    }

    if (!role || !allowedRoles.map(r => r.toLowerCase()).includes(role.toLowerCase())) {
      router.navigate(['/auth/login']);
      return false;
    }

    return true;
  };
}