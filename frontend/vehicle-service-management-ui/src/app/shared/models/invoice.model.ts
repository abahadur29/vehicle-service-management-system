export interface PartUsageDetail {
  partName: string;
  quantity: number;
  pricePerUnit: number;
  totalPrice?: number;
  PartName?: string;
  Quantity?: number;
  PricePerUnit?: number;
  TotalPrice?: number;
}

export interface Invoice {
  invoiceId: number;
  serviceRequestId: number;
  totalAmount: number;
  labourFee?: number; 
  issuedDate: string;
  paymentStatus: string;
  partsUsed: PartUsageDetail[];
  id?: number;
  InvoiceId?: number;
  ServiceRequestId?: number;
  TotalAmount?: number;
  LabourFee?: number;
  IssuedDate?: string;
  PaymentStatus?: string;
  PartsUsed?: PartUsageDetail[];
}
