import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { ServiceRequest, CreateServiceRequest } from '../../shared/models/service-request.model';
import { Notification } from '../../shared/models/notification.model';
import type { UpdateServiceStatusRequest, CompleteServiceRequest, BookServiceResponse, ApiResponse } from '../../shared/models/backend-response.model';

@Injectable({ providedIn: 'root' })
export class ServiceRequestService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/Services`;

  getMyBookings(): Observable<ServiceRequest[]> {
    return this.http.get<ServiceRequest[]>(this.baseUrl);
  }

  getAllRequests(): Observable<ServiceRequest[]> {
    return this.http.get<ServiceRequest[]>(`${this.baseUrl}/all`);
  }

  getTechnicianTasks(): Observable<ServiceRequest[]> {
    return this.http.get<ServiceRequest[]>(`${this.baseUrl}/tasks`);
  }

  getTechnicianServiceHistory(): Observable<ServiceRequest[]> {
    return this.http.get<ServiceRequest[]>(`${this.baseUrl}/history`);
  }

  bookService(request: CreateServiceRequest): Observable<ApiResponse<BookServiceResponse>> {
    const payload = {
      VehicleId: request.vehicleId,
      ServiceCategoryId: request.serviceCategoryId,
      Description: request.description,
      Priority: request.priority || 'Normal',
      RequestedDate: request.requestedDate,
    };
    return this.http.post<ApiResponse<BookServiceResponse>>(this.baseUrl, payload);
  }

  assignTechnician(data: { requestId: number; status: string; technicianId?: string; priority?: string }): Observable<ApiResponse> {
    const payload: UpdateServiceStatusRequest = {
      RequestId: data.requestId,
      Status: data.status,
    };
    if (data.technicianId) {
      payload.TechnicianId = data.technicianId;
    }
    if (data.priority) {
      payload.Priority = data.priority;
    }
    return this.http.post<ApiResponse>(`${this.baseUrl}/assign`, payload);
  }

  completeService(data: { requestId: number; partsUsed: Array<{ partId: number; quantity: number }>; remarks: string }): Observable<ApiResponse> {
    const payload: CompleteServiceRequest = {
      RequestId: data.requestId,
      PartsUsed: data.partsUsed.map(p => ({ PartId: p.partId, Quantity: p.quantity })),
      Remarks: data.remarks
    };
    return this.http.post<ApiResponse>(`${this.baseUrl}/complete`, payload);
  }

  cancelBooking(id: number): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${this.baseUrl}/${id}`);
  }

  rescheduleService(id: number, newRequestedDate: string): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.baseUrl}/${id}/reschedule`, { newRequestedDate });
  }

  getNotifications(): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.baseUrl}/notifications`);
  }
}
