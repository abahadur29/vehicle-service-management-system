export interface BackendInvoiceResponse {
  id?: number;
  Id?: number;
  InvoiceId?: number;
  invoiceId?: number;
  ServiceRequestId?: number;
  serviceRequestId?: number;
  serviceId?: number;
  ServiceId?: number;
  TotalAmount?: number;
  totalAmount?: number;
  amount?: number;
  Amount?: number;
  LabourFee?: number;
  labourFee?: number;
  laborFee?: number;
  LaborFee?: number;
  IssuedDate?: string;
  issuedDate?: string;
  date?: string;
  Date?: string;
  PaymentStatus?: string;
  paymentStatus?: string;
  status?: string;
  Status?: string;
  PartsUsed?: BackendPartUsageResponse[];
  partsUsed?: BackendPartUsageResponse[];
  items?: BackendPartUsageResponse[];
  Items?: BackendPartUsageResponse[];
}

export interface BackendPartUsageResponse {
  PartName?: string;
  partName?: string;
  name?: string;
  Name?: string;
  Quantity?: number;
  quantity?: number;
  Qty?: number;
  qty?: number;
  PricePerUnit?: number;
  pricePerUnit?: number;
  UnitPrice?: number;
  unitPrice?: number;
  price?: number;
  Price?: number;
  TotalPrice?: number;
  totalPrice?: number;
  total?: number;
  Total?: number;
}

export interface BackendAuthResponse {
  token?: string;
  Token?: string;
  role?: string;
  Role?: string;
  userId?: string;
  UserId?: string;
  email?: string;
  Email?: string;
  fullName?: string;
  FullName?: string;
  isSuccess?: boolean;
  IsSuccess?: boolean;
  message?: string;
  Message?: string;
}

export interface BackendServiceRequestResponse {
  id?: number;
  Id?: number;
  serviceRequestId?: number;
  ServiceRequestId?: number;
  serviceId?: number;
  ServiceId?: number;
}

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  phoneNumber?: string;
  role: string;
}

export interface ManageCategoryRequest {
  id: number;
  name: string;
  basePrice: number;
  description?: string;
  isActive: boolean;
}

export interface UpdateServiceStatusRequest {
  RequestId: number;
  Status: string;
  TechnicianId?: string;
  Priority?: string;
}

export interface CompleteServiceRequest {
  RequestId: number;
  PartsUsed: Array<{ PartId: number; Quantity: number }>;
  Remarks: string;
}

export interface BookServiceResponse {
  requestId: number;
  message: string;
}

export interface ApiResponse<T = unknown> {
  message?: string;
  data?: T;
  isSuccess?: boolean;
}

