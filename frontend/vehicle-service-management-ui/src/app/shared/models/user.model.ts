export type UserRole = 'Admin' | 'Manager' | 'Technician' | 'Customer';

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  phoneNumber?: string;
}

export interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  isSuccess: boolean;
  message: string;
  userId: string;
  role: UserRole | string;
}

export interface UserManagement extends User {
  activeTasks?: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  phoneNumber: string;
  role: string;
}
