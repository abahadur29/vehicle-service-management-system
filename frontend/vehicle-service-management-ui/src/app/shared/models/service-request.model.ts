export interface ServiceCategory {
  id: number;
  name: string;
  basePrice: number;
  description?: string;
  isActive?: boolean;
}

export interface ServiceRequest {
  id: number;
  description: string;
  status: string;
  priority: string;
  requestedDate: string;
  completionDate?: string;
  vehicleId: number;
  vehicleModel: string;
  technicianName?: string;
  technicianId?: string;
  invoiceId?: number;
}

export interface CreateServiceRequest {
  vehicleId: number;
  serviceCategoryId: number;
  description: string;
  priority: string;
  requestedDate: string;
}
