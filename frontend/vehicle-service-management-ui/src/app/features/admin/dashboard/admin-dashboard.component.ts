import { ChangeDetectionStrategy, Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';

import { AdminApiService } from '../../../core/services/admin-api.service';
import { TokenService } from '../../../core/auth/services/token.service';
import { DashboardService } from '../../../core/services/dashboard.service';

import type { Part } from '../../../shared/models/part.model';
import type { UserManagement } from '../../../shared/models/user-management.model';
import type {
  TechnicianWorkload,
  MonthlyRevenue,
  StatusCount,
  PendingVsCompleted
} from '../../../shared/models/dashboard.model';

interface UserRoleDistribution {
  roleName: string;
  userCount: number;
}

interface MonthlyServiceVolume {
  year: number;
  month: number;
  serviceCount: number;
  formattedPeriod: string;
}

@Component({
  standalone: true,
  selector: 'app-admin-dashboard',
  imports: [CommonModule, MatCardModule, MatGridListModule, MatTableModule, MatIconModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardComponent implements OnInit {
  private api = inject(AdminApiService);
  private token = inject(TokenService);
  private dashboardService = inject(DashboardService);

  role = (this.token.getRole() ?? '').toLowerCase();
  isAdmin = computed(() => this.role === 'admin');
  isManager = computed(() => this.role === 'manager');
  lowStock = signal<Part[]>([]);
  technicians = signal<UserManagement[]>([]);
  roleDistribution = signal<UserRoleDistribution[]>([]);
  
  technicianWorkload = signal<TechnicianWorkload[]>([]);
  servicesByStatus = signal<StatusCount[]>([]);
  monthlyServiceVolume = signal<MonthlyServiceVolume[]>([]);
  pendingVsCompleted = signal<PendingVsCompleted>({ pendingCount: 0, completedCount: 0, statusBreakdown: [] });

  lowStockCount = computed(() => this.lowStock().length);
  availableTechCount = computed(() => this.technicians().length);
  
  totalUsersCount = computed(() => {
    const distribution = this.roleDistribution();
    if (distribution.length > 0) {
      const roleCounts = distribution.reduce((sum, role) => sum + role.userCount, 0);
      return roleCounts + 1;
    }
    return 0;
  });
  
  maxUserCount = computed(() => {
    const counts = this.roleDistribution().map(r => r.userCount);
    return Math.max(...counts, 1); // At least 1 to avoid division by zero
  });

  maxWorkload = computed(() => {
    const workloads = this.technicianWorkload().map(w => w.activeTasks);
    return workloads.length > 0 ? Math.max(...workloads, 1) : 1;
  });

  maxMonthlyVolume = computed(() => {
    const volumes = this.monthlyServiceVolume().map(v => v.serviceCount);
    return Math.max(...volumes, 1);
  });

  stockColumns = ['name', 'stockQuantity', 'unitPrice'];

  ngOnInit() {
    this.api.getLowStock().subscribe((p) => this.lowStock.set(p));
    this.api.getTechnicians().subscribe((t) => this.technicians.set(t));
    
    if (this.isAdmin()) {
      this.api.getUserRoleDistribution().subscribe({
        next: (r) => {
          this.roleDistribution.set(r);
        },
        error: (err) => {
          console.error('Failed to load user role distribution:', err);
        }
      });
    }

    if (this.isManager()) {
      this.dashboardService.getTechnicianWorkload().subscribe({
        next: (workload) => this.technicianWorkload.set(workload || []),
        error: (err) => console.error('Failed to load technician workload:', err)
      });

      this.dashboardService.getPendingVsCompleted().subscribe({
        next: (data) => {
          this.pendingVsCompleted.set(data);
          this.servicesByStatus.set(data.statusBreakdown || []);
        },
        error: (err) => console.error('Failed to load services by status:', err)
      });

      this.dashboardService.getMonthlyStats().subscribe({
        next: (monthly) => {
          const formatted: MonthlyServiceVolume[] = (monthly || []).map((m: MonthlyRevenue) => ({
            year: m.year || new Date().getFullYear(),
            month: m.month || new Date().getMonth() + 1,
            serviceCount: m.serviceCount || 0,
            formattedPeriod: this.formatMonthYear(m.month, m.year)
          })).slice(-6); // Last 6 months
          this.monthlyServiceVolume.set(formatted);
        },
        error: (err) => console.error('Failed to load monthly service volume:', err)
      });
    }
  }

  private formatMonthYear(month: number | string, year: number | string): string {
    const monthNum = typeof month === 'number' ? month : parseInt(month.toString(), 10);
    const yearNum = typeof year === 'number' ? year : parseInt(year.toString(), 10);
    
    if (isNaN(monthNum) || isNaN(yearNum) || monthNum < 1 || monthNum > 12) {
      return `${month} ${year}`;
    }

    const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    return `${monthNames[monthNum - 1]} ${yearNum}`;
  }
}