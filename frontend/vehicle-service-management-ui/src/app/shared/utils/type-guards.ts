import type { Invoice } from '../models/invoice.model';
import type { Vehicle } from '../models/vehicle.model';
import type { Part } from '../models/part.model';
import type { ServiceRequest } from '../models/service-request.model';

export function isInvoice(obj: unknown): obj is Invoice {
  return (
    typeof obj === 'object' &&
    obj !== null &&
    'invoiceId' in obj &&
    'serviceRequestId' in obj &&
    'totalAmount' in obj &&
    'paymentStatus' in obj
  );
}

export function isVehicle(obj: unknown): obj is Vehicle {
  return (
    typeof obj === 'object' &&
    obj !== null &&
    'id' in obj &&
    'licensePlate' in obj &&
    'make' in obj &&
    'model' in obj &&
    'year' in obj
  );
}

export function isPart(obj: unknown): obj is Part {
  return (
    typeof obj === 'object' &&
    obj !== null &&
    'id' in obj &&
    'name' in obj &&
    'stockQuantity' in obj
  );
}

export function isServiceRequest(obj: unknown): obj is ServiceRequest {
  return (
    typeof obj === 'object' &&
    obj !== null &&
    'id' in obj &&
    'status' in obj &&
    'vehicleId' in obj
  );
}

export function isNumber(value: unknown): value is number {
  return typeof value === 'number' && !isNaN(value);
}

export function isString(value: unknown): value is string {
  return typeof value === 'string';
}

export function isArray<T>(value: unknown, itemGuard?: (item: unknown) => item is T): value is T[] {
  if (!Array.isArray(value)) {
    return false;
  }
  if (itemGuard) {
    return value.every(item => itemGuard(item));
  }
  return true;
}

export function isValidDate(value: unknown): value is Date {
  return value instanceof Date && !isNaN(value.getTime());
}

export function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

export function isValidPhoneNumber(phone: string): boolean {
  const phoneRegex = /^[+]?[(]?[0-9]{1,4}[)]?[-\s.]?[(]?[0-9]{1,4}[)]?[-\s.]?[0-9]{1,9}$/;
  return phoneRegex.test(phone);
}

