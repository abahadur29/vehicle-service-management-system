import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit, ViewChild, AfterViewInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { FormsModule } from '@angular/forms';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { DashboardService } from '../../../../core/services/dashboard.service';
import { VehicleService } from '../../../../core/services/vehicle.service';
import { Observable, forkJoin, map } from 'rxjs';
import { MonthYearFormatPipe } from '../../../../shared/pipes/month-year-format.pipe';
import type { ReportData, VehicleHistoryItem, TechPerformanceNormalized, RevenueByTypeProcessed, MonthlyRevenueFormatted, VehicleSelectionItem } from '../../../../shared/models/report.model';
import type { VehicleServiceHistory, TechPerformance, ServiceTypeRevenue, MonthlyRevenue } from '../../../../shared/models/dashboard.model';

@Component({
  standalone: true,
  selector: 'app-report-list',
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatTabsModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatAutocompleteModule,
    MatPaginatorModule,
    MatSortModule,
    FormsModule
  ],
  templateUrl: './report-list.component.html',
  styleUrls: ['./report-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportListComponent implements OnInit, AfterViewInit {
  private dashboardService = inject(DashboardService);
  private vehicleService = inject(VehicleService);
  private cdr = inject(ChangeDetectorRef);

  reports$: Observable<ReportData> | undefined;
  
  selectedVehicleId = signal<number | null>(null);
  vehicleHistoryDataSource = new MatTableDataSource<VehicleHistoryItem>([]);
  vehicleHistoryDisplayedColumns: string[] = ['serviceName', 'completionDate', 'description', 'partsReplaced', 'totalCost'];
  vehicles: VehicleSelectionItem[] = [];
  filteredVehicles: VehicleSelectionItem[] = [];
  vehicleSearchInput = '';
  
  @ViewChild(MatPaginator) vehicleHistoryPaginator!: MatPaginator;
  @ViewChild(MatSort) vehicleHistorySort!: MatSort;

  ngOnInit(): void {
    this.vehicleHistoryDataSource.sortingDataAccessor = (item: VehicleHistoryItem, property: string) => {
      switch (property) {
        case 'completionDate':
          const dateStr = item.completionDate || '';
          if (!dateStr) return 0;
          const date = new Date(dateStr);
          const timestamp = date.getTime();
          return isNaN(timestamp) ? 0 : timestamp;
        case 'totalCost':
          return Number(item.totalCost) || 0;
        case 'partsReplaced':
          const parts = item.partsReplaced || [];
          return Array.isArray(parts) ? parts.join(', ').toLowerCase() : '';
        default:
          return (item as any)[property] || '';
      }
    };

    this.vehicleHistoryDataSource.filterPredicate = (data: VehicleHistoryItem, filter: string) => {
      const searchStr = filter.toLowerCase();
      const serviceName = (data.serviceName || '').toLowerCase();
      const description = (data.description || '').toLowerCase();
      const partsReplaced = Array.isArray(data.partsReplaced) 
        ? data.partsReplaced.join(' ').toLowerCase() 
        : '';
      const totalCost = (data.totalCost || '').toString().toLowerCase();
      // Check if search matches date in different formats
      const dateStr = data.completionDate || '';
      let dateMatch = false;
      if (dateStr) {
        const date = new Date(dateStr);
        if (!isNaN(date.getTime())) {
          // Try different date formats for search
          const day = date.getDate().toString().padStart(2, '0');
          const month = (date.getMonth() + 1).toString().padStart(2, '0');
          const year = date.getFullYear().toString();
          const ddMMyyyy = `${day}/${month}/${year}`;
          const dMyyyy = `${date.getDate()}/${month}/${year}`;
          const ddMyyyy = `${day}/${date.getMonth() + 1}/${year}`;
          const formattedDate = date.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric'
          }).toLowerCase();
          const shortDate = date.toLocaleDateString('en-US', {
            month: 'numeric',
            day: 'numeric',
            year: 'numeric'
          }).toLowerCase();
          const longDate = date.toLocaleDateString('en-US', {
            month: 'long',
            day: 'numeric',
            year: 'numeric'
          }).toLowerCase();
          dateMatch = ddMMyyyy.includes(searchStr) ||
                     dMyyyy.includes(searchStr) ||
                     ddMyyyy.includes(searchStr) ||
                     formattedDate.includes(searchStr) ||
                     shortDate.includes(searchStr) ||
                     longDate.includes(searchStr);
        }
      }
      
      return serviceName.includes(searchStr) ||
             description.includes(searchStr) ||
             partsReplaced.includes(searchStr) ||
             totalCost.includes(searchStr) ||
             dateMatch;
    };

    this.reports$ = forkJoin({
      techPerformance: this.dashboardService.getTechPerformance(),
      revenueByType: this.dashboardService.getRevenueByServiceType(),
      monthly: this.dashboardService.getMonthlyStats(),
      pendingVsCompleted: this.dashboardService.getPendingVsCompleted(),
      vehicleServiceHistory: this.dashboardService.getVehicleServiceHistoryReport(),
    }).pipe(
      map(data => {
        const normalizedTechPerformance: TechPerformanceNormalized[] = data.techPerformance
          .map((t: TechPerformance) => ({
            technicianName: t.technicianName || 'Unknown',
            totalRevenue: Number(t.totalRevenueGenerated || 0),
            completedJobs: Number(t.jobsCompleted || 0)
          }))
          .filter((t) => t.totalRevenue > 0)
          .sort((a, b) => b.totalRevenue - a.totalRevenue);

        const maxTechRevenue = normalizedTechPerformance.length > 0
          ? Math.max(...normalizedTechPerformance.map((t) => t.totalRevenue))
          : 1;

        const maxMonthlyRevenue = Math.max(...data.monthly.map((m: MonthlyRevenue) => Number(m.revenue || 0)), 1);
        
        const processedRevenueByType: RevenueByTypeProcessed[] = (data.revenueByType || []).map((r: ServiceTypeRevenue) => ({
          serviceType: r.serviceType || 'Other',
          revenue: Number(r.revenue || 0),
          serviceCount: r.serviceCount || 0
        }));

        const maxServiceRevenue = Math.max(...processedRevenueByType.map((r) => r.revenue), 1);
        
        const activeRevenueByType = processedRevenueByType.filter((r) => r.revenue > 0);
        const totalServiceRevenue = activeRevenueByType.reduce((sum: number, r) => sum + r.revenue, 0);
        
        const thresholdPercent = 5;
        const mainCategories: RevenueByTypeProcessed[] = [];
        const othersCategories: RevenueByTypeProcessed[] = [];
        let othersRevenue = 0;
        
        activeRevenueByType.forEach((r) => {
          const percent = totalServiceRevenue > 0 ? (r.revenue / totalServiceRevenue) * 100 : 0;
          if (percent >= thresholdPercent) {
            mainCategories.push({ ...r, percent });
          } else {
            othersCategories.push(r);
            othersRevenue += r.revenue;
          }
        });
        
        if (othersCategories.length > 0 && othersRevenue > 0) {
          const othersPercent = totalServiceRevenue > 0 ? (othersRevenue / totalServiceRevenue) * 100 : 0;
          mainCategories.push({
            serviceType: 'Others',
            revenue: othersRevenue,
            percent: othersPercent,
            serviceCount: othersCategories.reduce((sum, c) => sum + (c.serviceCount || 0), 0),
            isOthers: true
          });
        }
        
        mainCategories.sort((a, b) => b.revenue - a.revenue);
        
        let cumulativePercent = 0;
        const revenueByTypeWithPercents = mainCategories.map((r) => {
          const startPercent = cumulativePercent;
          cumulativePercent += r.percent || 0;
          return { ...r, startPercent };
        });
        const formattedMonthly: MonthlyRevenueFormatted[] = data.monthly
          .sort((a: MonthlyRevenue, b: MonthlyRevenue) => (a.year * 12 + a.month) - (b.year * 12 + b.month))
          .map((m: MonthlyRevenue) => ({
            year: m.year,
            month: m.month,
            revenue: Number(m.revenue || 0),
            formattedPeriod: this.formatMonthYear(m.month, m.year)
          })).slice(-6);
        const sortedVehicleHistory = (data.vehicleServiceHistory || [])
          .sort((a: VehicleServiceHistory, b: VehicleServiceHistory) => {
            const dateA = a.lastServiceDate ? new Date(a.lastServiceDate).getTime() : 0;
            const dateB = b.lastServiceDate ? new Date(b.lastServiceDate).getTime() : 0;
            return dateB - dateA; // Descending order (most recent first)
          });

        this.vehicles = sortedVehicleHistory.map(v => ({
          id: v.vehicleId,
          display: `${v.licensePlate} - ${v.make} ${v.model} (${v.year})`,
          licensePlate: v.licensePlate,
          make: v.make,
          model: v.model,
          year: v.year
        }));
        this.filteredVehicles = [];

        return {
          ...data,
          techPerformance: normalizedTechPerformance,
          revenueByType: revenueByTypeWithPercents,
          monthly: formattedMonthly,
          vehicleServiceHistory: sortedVehicleHistory,
          maxTechRevenue,
          maxServiceRevenue,
          maxMonthlyRevenue,
          totalServiceRevenue
        };
      })
    );
  }

  ngAfterViewInit() {
    if (this.vehicleHistoryPaginator) {
      this.vehicleHistoryDataSource.paginator = this.vehicleHistoryPaginator;
    }
    if (this.vehicleHistorySort) {
      this.vehicleHistoryDataSource.sort = this.vehicleHistorySort;
    }
    
    this.cdr.detectChanges();
  }

  onVehicleSelected(vehicleId: number) {
    this.selectedVehicleId.set(vehicleId);
    this.loadVehicleHistory(vehicleId);
  }

  loadVehicleHistory(vehicleId: number) {
    this.vehicleService.getVehicleHistory(vehicleId).subscribe({
      next: (history) => {
        this.vehicleHistoryDataSource.data = history;
        
        if (this.vehicleHistoryPaginator) {
          this.vehicleHistoryDataSource.paginator = this.vehicleHistoryPaginator;
        }
        
        if (this.vehicleHistorySort) {
          this.vehicleHistorySort.active = 'completionDate';
          this.vehicleHistorySort.direction = 'desc';
          this.vehicleHistoryDataSource.sort = this.vehicleHistorySort;
        }
        
        if (this.vehicleHistoryPaginator) {
          this.vehicleHistoryPaginator.firstPage();
        }
        
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading vehicle history:', err);
        this.vehicleHistoryDataSource.data = [];
        if (this.vehicleHistoryPaginator) {
          this.vehicleHistoryDataSource.paginator = this.vehicleHistoryPaginator;
        }
        this.cdr.detectChanges();
      }
    });
  }

  applyVehicleSearch() {
    const searchStr = this.vehicleSearchInput.trim().toLowerCase();
    
    if (!searchStr) {
      this.filteredVehicles = [];
    } else {
      this.filteredVehicles = this.vehicles.filter(v => 
        (v.licensePlate || '').toLowerCase().includes(searchStr) ||
        (v.make || '').toLowerCase().includes(searchStr) ||
        (v.model || '').toLowerCase().includes(searchStr) ||
        (v.year || '').toString().includes(searchStr)
      );
    }
  }

  onVehicleSelectedFromSearch(vehicleId: number) {
    if (vehicleId) {
      const selected = this.vehicles.find(v => v.id === vehicleId);
      if (selected) {
        this.vehicleSearchInput = selected.display;
      }
      this.selectedVehicleId.set(vehicleId);
      this.loadVehicleHistory(vehicleId);
      this.filteredVehicles = [];
    }
  }

  applyVehicleHistoryFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.vehicleHistoryDataSource.filter = filterValue.trim().toLowerCase();
    
    if (this.vehicleHistoryPaginator) {
      this.vehicleHistoryDataSource.paginator = this.vehicleHistoryPaginator;
      this.vehicleHistoryPaginator.firstPage();
    }
  }

  private formatMonthYear(month: number | string, year: number | string): string {
    const monthNum = typeof month === 'number' ? month : parseInt(month, 10);
    const yearNum = typeof year === 'number' ? year : parseInt(year, 10);
    
    if (isNaN(monthNum) || isNaN(yearNum) || monthNum < 1 || monthNum > 12) {
      return `${month} ${year}`;
    }

    const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    return `${monthNames[monthNum - 1]} ${yearNum}`;
  }

  getPieGradient(data: RevenueByTypeProcessed[]): string {
    if (!data || data.length === 0) return 'transparent';
    
    const colors = ['#6366f1', '#8b5cf6', '#ec4899', '#f43f5e', '#f59e0b', '#10b981'];
    const segments = data.map((r, i) => {
      const color = r.isOthers ? '#94a3b8' : colors[i % colors.length];
      return `${color} ${r.startPercent || 0}% ${(r.startPercent || 0) + (r.percent || 0)}%`;
    });
    
    return `conic-gradient(${segments.join(', ')})`;
  }

  getColor(index: number, item?: RevenueByTypeProcessed): string {
    if (item?.isOthers) {
      return '#94a3b8';
    }
    const colors = ['#6366f1', '#8b5cf6', '#ec4899', '#f43f5e', '#f59e0b', '#10b981'];
    return colors[index % colors.length];
  }

  formatCurrency(amount: number): string {
    return `₹${amount.toLocaleString('en-IN', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
  }
}
