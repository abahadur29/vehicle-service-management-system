import { Routes } from '@angular/router';
import { authGuard } from './core/auth/guards/auth.guard';
import { roleGuard } from './core/auth/guards/role.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'auth/login' },

  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },

  {
    path: '',
    loadComponent: () =>
      import('./core/auth/layout/components/shell/shell.component').then((m) => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.routes').then((m) => m.DASHBOARD_ROUTES),
      },
      {
        path: 'vehicles',
        loadChildren: () => import('./features/vehicles/vehicles.routes').then((m) => m.VEHICLE_ROUTES),
      },
      {
        path: 'service-requests',
        loadChildren: () => import('./features/service-requests/service-requests.routes').then((m) => m.SERVICE_REQUEST_ROUTES),
      },
      {
        path: 'billing',
        loadChildren: () => import('./features/billing/billing.routes').then((m) => m.BILLING_ROUTES),
      },
      {
        path: 'admin',
        canActivate: [roleGuard(['Admin', 'Manager'])],
        loadChildren: () => import('./features/admin/admin.routes').then((m) => m.ADMIN_ROUTES),
      },
      {
        path: 'reports',
        canActivate: [roleGuard(['Manager'])],
        loadChildren: () => import('./features/reports/reports.routes').then((m) => m.REPORT_ROUTES),
      },
    ],
  },

  { path: '**', redirectTo: 'auth/login' },
];