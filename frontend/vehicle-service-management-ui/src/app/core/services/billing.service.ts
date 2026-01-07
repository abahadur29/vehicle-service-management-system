import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, map } from 'rxjs';
import { Invoice } from '../../shared/models/invoice.model';
import type { BackendInvoiceResponse, BackendPartUsageResponse, ApiResponse } from '../../shared/models/backend-response.model';

@Injectable({ providedIn: 'root' })
export class BillingService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/Billing`;

  getInvoices(): Observable<Invoice[]> {
    return this.http.get<BackendInvoiceResponse[] | ApiResponse<BackendInvoiceResponse[]>>(this.baseUrl).pipe(
      map(response => {
        const invoices = Array.isArray(response) ? response : (response?.data || []);
        return invoices.map((inv) => this.mapInvoice(inv));
      })
    );
  }

  getInvoiceByServiceId(serviceId: number): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.baseUrl}/invoice/${serviceId}`).pipe(
      map(inv => this.mapInvoice(inv))
    );
  }

  payInvoice(invoiceId: number): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/pay/${invoiceId}`, {});
  }

  private mapInvoice(inv: BackendInvoiceResponse): Invoice {
    if (!inv) {
      return {
        invoiceId: 0,
        serviceRequestId: 0,
        totalAmount: 0,
        issuedDate: new Date().toISOString(),
        paymentStatus: 'Pending',
        partsUsed: []
      };
    }
    const rawAmount = inv.totalAmount || inv.TotalAmount || inv.amount || inv.Amount || 0;
    const totalAmount = typeof rawAmount === 'number' ? rawAmount : Number(rawAmount) || 0;
    const rawLabourFee = inv.labourFee || inv.LabourFee || inv.laborFee || inv.LaborFee || 0;
    const labourFee = typeof rawLabourFee === 'number' ? rawLabourFee : Number(rawLabourFee) || 0;
    
    return {
      invoiceId: inv.invoiceId || inv.InvoiceId || inv.id || inv.Id || 0,
      serviceRequestId: inv.serviceRequestId || inv.ServiceRequestId || inv.serviceId || inv.ServiceId || 0,
      totalAmount: totalAmount,
      labourFee: labourFee,
      issuedDate: inv.issuedDate || inv.IssuedDate || inv.date || inv.Date || new Date().toISOString(),
      paymentStatus: inv.paymentStatus || inv.PaymentStatus || inv.status || inv.Status || 'Pending',
      partsUsed: (inv.partsUsed || inv.PartsUsed || inv.items || inv.Items || []).map((p: BackendPartUsageResponse) => ({
        partName: p.partName || p.PartName || p.name || p.Name || 'Unknown Part',
        quantity: typeof p.quantity === 'number' ? p.quantity : (p.Quantity || p.qty || p.Qty || 0),
        pricePerUnit: typeof p.pricePerUnit === 'number' ? p.pricePerUnit : (Number(p.PricePerUnit || p.unitPrice || p.UnitPrice || p.price || p.Price) || 0),
        unitPrice: typeof p.unitPrice === 'number' ? p.unitPrice : (Number(p.UnitPrice || p.pricePerUnit || p.PricePerUnit || p.price || p.Price) || 0),
        totalPrice: typeof p.totalPrice === 'number' ? p.totalPrice : (Number(p.TotalPrice || p.total || p.Total) || 0)
      }))
    };
  }
}
