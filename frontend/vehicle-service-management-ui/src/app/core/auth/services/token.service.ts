import { Injectable } from '@angular/core';

const TOKEN_KEY = 'vsm_token';
const ROLE_KEY = 'vsm_role';
const USER_ID_KEY = 'vsm_user_id';
const EMAIL_KEY = 'vsm_email';
const FULLNAME_KEY = 'vsm_fullname';

@Injectable({ providedIn: 'root' })
export class TokenService {
  setSession(data: { token: string; role: string; userId: string; email: string; fullName: string }) {
    localStorage.setItem(TOKEN_KEY, data.token);
    localStorage.setItem(ROLE_KEY, data.role);
    localStorage.setItem(USER_ID_KEY, data.userId);
    localStorage.setItem(EMAIL_KEY, data.email);
    localStorage.setItem(FULLNAME_KEY, data.fullName);
  }

  clear() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(USER_ID_KEY);
    localStorage.removeItem(EMAIL_KEY);
    localStorage.removeItem(FULLNAME_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getRole(): string | null {
    return localStorage.getItem(ROLE_KEY);
  }

  getUserId(): string | null {
    return localStorage.getItem(USER_ID_KEY);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // Convert to milliseconds
      if (Date.now() >= exp) {
        this.clear(); // Token expired, clear it
        return false;
      }
      return true;
    } catch {
      return false;
    }
  }
}