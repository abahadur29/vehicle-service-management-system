/**
 * Dashboard and Reports DTOs
 * These interfaces match the backend DTOs for type safety
 */

export interface TechnicianWorkload {
  technicianId: string;
  technicianName: string;
  activeTasks: number; // Maps to ActiveTasksCount from backend
  currentVehicleModels: string[];
}

export interface MonthlyRevenue {
  year: number;
  month: number;
  revenue: number;
  serviceCount: number;
}

export interface StatusCount {
  status: string;
  count: number;
}

export interface PendingVsCompleted {
  pendingCount: number;
  completedCount: number;
  statusBreakdown: StatusCount[];
}

export interface TechPerformance {
  technicianName: string;
  totalRevenueGenerated: number;
  jobsCompleted: number;
}

export interface ServiceTypeRevenue {
  serviceType: string; // Maps to ServiceTypeName from backend
  revenue: number; // Maps to TotalRevenue from backend
  serviceCount: number;
}

export interface InvoiceSummary {
  invoiceId: number;
  customerVehicle: string;
  amount: number;
  date: string; // ISO date string
  status: string;
}

export interface RevenueReport {
  totalRevenue: number;
  totalServicesCompleted: number;
  recentInvoices: InvoiceSummary[];
}

export interface VehicleServiceHistory {
  vehicleId: number;
  make: string;
  model: string;
  licensePlate: string;
  year: number;
  totalServices: number;
  lastServiceDate: string | null; // ISO date string or null
  lastServiceStatus: string;
}

