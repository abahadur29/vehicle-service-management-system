import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Vehicle } from '../../shared/models/vehicle.model';
import type { VehicleHistoryItem } from '../../shared/models/report.model';
import type { ApiResponse } from '../../shared/models/backend-response.model';

@Injectable({ providedIn: 'root' })
export class VehicleService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/Vehicles`;

  getVehicles(): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(`${this.baseUrl}/my-vehicles`);
  }

  addVehicle(vehicle: Omit<Vehicle, 'id' | 'ownerFullName'>): Observable<ApiResponse<Vehicle>> {
    return this.http.post<ApiResponse<Vehicle>>(this.baseUrl, vehicle);
  }

  getVehicleHistory(id: number): Observable<VehicleHistoryItem[]> {
    return this.http.get<VehicleHistoryItem[]>(`${this.baseUrl}/${id}/history`);
  }
}
