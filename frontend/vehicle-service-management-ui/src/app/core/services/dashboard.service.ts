import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import type {
  TechnicianWorkload,
  RevenueReport,
  MonthlyRevenue,
  TechPerformance,
  ServiceTypeRevenue,
  PendingVsCompleted,
  VehicleServiceHistory
} from '../../shared/models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/Reports`;

  getTechnicianWorkload(): Observable<TechnicianWorkload[]> {
    return this.http.get<TechnicianWorkload[]>(`${this.baseUrl}/technician-workload`);
  }

  getRevenueReport(): Observable<RevenueReport> {
    return this.http.get<RevenueReport>(`${this.baseUrl}/revenue-report`);
  }

  getMonthlyStats(): Observable<MonthlyRevenue[]> {
    return this.http.get<MonthlyRevenue[]>(`${this.baseUrl}/monthly-stats`);
  }

  getTechPerformance(): Observable<TechPerformance[]> {
    return this.http.get<TechPerformance[]>(`${this.baseUrl}/tech-performance`);
  }

  getRevenueByServiceType(): Observable<ServiceTypeRevenue[]> {
    return this.http.get<ServiceTypeRevenue[]>(`${this.baseUrl}/revenue-by-type`);
  }

  getPendingVsCompleted(): Observable<PendingVsCompleted> {
    return this.http.get<PendingVsCompleted>(`${this.baseUrl}/pending-vs-completed`);
  }

  getVehicleServiceHistoryReport(): Observable<VehicleServiceHistory[]> {
    return this.http.get<VehicleServiceHistory[]>(`${this.baseUrl}/vehicle-service-history`);
  }
}
