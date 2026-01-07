import { ChangeDetectionStrategy, Component, computed, inject, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';


import type { NavItem } from '../../../../../shared/models/nav-item.model';
import { TokenService } from '../../../services/token.service';

@Component({
  standalone: true,
  selector: 'app-sidenav',
  imports: [CommonModule, RouterLink, RouterLinkActive, MatListModule, MatIconModule],
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidenavComponent {
  navigate = output<void>();

  private token = inject(TokenService);

  role = computed(() => (this.token.getRole() ?? '').toLowerCase());

  navItems = computed<NavItem[]>(() => {
    const r = this.role();

    if (r === 'admin') {
      return [
        { label: 'Dashboard', icon: 'dashboard', route: '/admin/dashboard' },
        { label: 'User Management', icon: 'people', route: '/admin/users' },
        { label: 'Categories', icon: 'category', route: '/admin/categories' },
        { label: 'Inventory', icon: 'inventory_2', route: '/admin/inventory' },
      ];
    }

    if (r === 'manager') {
      return [
        { label: 'Dashboard', icon: 'dashboard', route: '/admin/dashboard' },
        { label: 'Service Management', icon: 'assignment', route: '/admin/service-requests' },
        { label: 'Inventory', icon: 'inventory_2', route: '/admin/inventory' },
        { label: 'Reports & Analytics', icon: 'bar_chart', route: '/reports' },
      ];
    }

    if (r === 'customer') {
      return [
        { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
        { label: 'My Vehicles', icon: 'directions_car', route: '/vehicles' },
        { label: 'My Services', icon: 'build', route: '/service-requests/my-requests' },
        { label: 'Billing', icon: 'receipt', route: '/billing' },
      ];
    }

    if (r === 'technician') {
      return [
        { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
        { label: 'My Tasks', icon: 'assignment', route: '/service-requests/my-tasks' },
        { label: 'Service History', icon: 'history', route: '/service-requests/history' },
      ];
    }

    return [{ label: 'Home', icon: 'home', route: '/dashboard' }];
  });

  onNavigate() {
    this.navigate.emit();
  }
}