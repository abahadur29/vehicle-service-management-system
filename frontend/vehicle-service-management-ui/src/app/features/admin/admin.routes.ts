import { Routes } from '@angular/router';
import { AdminDashboardComponent } from './dashboard/admin-dashboard.component';
import { UserManagementComponent } from './users/user-management.component';
import { InventoryManagementComponent } from '../inventory/parts-list/inventory-management.component';
import { CategoryManagementComponent } from './categories/category-management.component';
import { ServiceManagementComponent } from './service-requests/service-management.component';
import { roleGuard } from '../../core/auth/guards/role.guard';

export const ADMIN_ROUTES: Routes = [
  { path: 'dashboard', component: AdminDashboardComponent },
  { 
    path: 'users', 
    component: UserManagementComponent,
    canActivate: [roleGuard(['Admin'])]
  },
  { 
    path: 'service-requests', 
    component: ServiceManagementComponent 
  },
  { 
    path: 'inventory', 
    component: InventoryManagementComponent,
    canActivate: [roleGuard(['Admin', 'Manager', 'Technician'])] // Technician gets read-only view without pricing
  },
  { 
    path: 'categories', 
    component: CategoryManagementComponent,
    canActivate: [roleGuard(['Admin'])]
  },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
];