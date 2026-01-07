import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { UserManagement } from '../../shared/models/user-management.model';
import type { UpdateRoleRequest } from '../../shared/models/update-role.model';
import type { Part } from '../../shared/models/part.model';
import { RevenueReport } from '../../shared/models/revenue-report.model';
import { ServiceCategory } from '../../shared/models/service-request.model';
import type { CreateUserRequest, ManageCategoryRequest, ApiResponse } from '../../shared/models/backend-response.model';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/Admin`;

  getUsers() {
    return this.http.get<UserManagement[]>(`${this.baseUrl}/users`);
  }

  updateRole(dto: UpdateRoleRequest) {
    return this.http.put(`${this.baseUrl}/update-role`, dto);
  }

  getRevenueReport() {
    return this.http.get<RevenueReport>(`${this.baseUrl}/revenue-report`);
  }

  getParts() {
    return this.http.get<Part[]>(`${this.baseUrl}/parts`);
  }

  getLowStock() {
    return this.http.get<Part[]>(`${this.baseUrl}/low-stock`);
  }

  updateStock(id: number, quantity: number) {
    return this.http.put(`${this.baseUrl}/update-stock/${id}`, { quantity });
  }

  getTechnicians() {
    return this.http.get<UserManagement[]>(`${this.baseUrl}/technicians`);
  }

  getCategories(activeOnly: boolean = false) {
    return this.http.get<ServiceCategory[]>(`${this.baseUrl}/categories?activeOnly=${activeOnly}`);
  }

  approveService(id: number) {
    return this.http.post(`${this.baseUrl}/approve-service/${id}`, {});
  }

  createUser(dto: CreateUserRequest) {
    return this.http.post<ApiResponse<{ userId: string }>>(`${this.baseUrl}/create-user`, dto);
  }

  deleteUser(userId: string) {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/users/${userId}`);
  }

  manageCategory(dto: ManageCategoryRequest) {
    return this.http.post<boolean>(`${this.baseUrl}/manage-category`, dto);
  }

  updatePrice(id: number, newPrice: number) {
    return this.http.put(`${this.baseUrl}/update-price/${id}`, { newPrice });
  }

  toggleActive(id: number) {
    return this.http.post(`${this.baseUrl}/toggle-active/${id}`, {});
  }

  toggleUserActive(userId: string) {
    return this.http.post(`${this.baseUrl}/toggle-user-active/${userId}`, {});
  }

  getUserRoleDistribution() {
    return this.http.get<{ roleName: string; userCount: number }[]>(`${this.baseUrl}/user-role-distribution`);
  }
}   