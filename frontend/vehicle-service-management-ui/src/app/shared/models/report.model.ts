export interface VehicleHistoryItem {
  serviceName: string;
  completionDate: string;
  description: string;
  partsReplaced: string;
  totalCost: number | string;
}

export interface TechPerformanceNormalized {
  technicianName: string;
  totalRevenue: number;
  completedJobs: number;
}

export interface RevenueByTypeProcessed {
  serviceType: string;
  revenue: number;
  serviceCount?: number;
  percent?: number;
  startPercent?: number;
  isOthers?: boolean;
}

export interface MonthlyRevenueFormatted {
  year: number;
  month: number;
  revenue: number;
  formattedPeriod: string;
}

export interface VehicleSelectionItem {
  id: number;
  display: string;
  licensePlate: string;
  make: string;
  model: string;
  year: number;
}

import type { VehicleServiceHistory } from './dashboard.model';

export interface ReportData {
  techPerformance: TechPerformanceNormalized[];
  monthly: MonthlyRevenueFormatted[];
  revenueByType: RevenueByTypeProcessed[];
  vehicleServiceHistory: VehicleServiceHistory[];
  pendingVsCompleted: {
    pendingCount: number;
    completedCount: number;
    statusBreakdown: Array<{ status: string; count: number }>;
  };
  maxTechRevenue?: number;
  maxServiceRevenue?: number;
  maxMonthlyRevenue?: number;
  totalServiceRevenue?: number;
}

