export interface UserManagement {
  id: string;
  fullName: string;
  email: string;
  role: string;
  currentWorkload: number;
  isActive?: boolean; // Track activation status locally
}