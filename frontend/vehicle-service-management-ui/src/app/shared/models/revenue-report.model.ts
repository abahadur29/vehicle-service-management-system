export interface InvoiceSummary {
  invoiceId: number;
  customerVehicle: string;
  amount: number;
  date: string;
  status: string;
}

export interface RevenueReport {
  totalRevenue: number;
  totalServicesCompleted: number;
  recentInvoices: InvoiceSummary[];
}