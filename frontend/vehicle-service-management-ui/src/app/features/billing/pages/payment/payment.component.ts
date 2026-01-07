import { ChangeDetectionStrategy, Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { BillingService } from '../../../../core/services/billing.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Invoice } from '../../../../shared/models/invoice.model';

@Component({
  standalone: true,
  selector: 'app-payment',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    RouterLink
  ],
  template: `
    <div class="payment-container">
      <mat-card class="payment-card">
        <mat-card-header>
          <mat-card-title>Secure Payment</mat-card-title>
          <mat-card-subtitle>Service Request #{{ serviceId }}</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <div *ngIf="!invoice" class="loading-message">
            <p>Loading invoice details...</p>
          </div>
          <div *ngIf="invoice" class="invoice-breakdown">
            <h3 style="margin-bottom: 16px;">Invoice Breakdown</h3>
            <div class="breakdown-item">
              <span>Parts Total:</span>
              <span>{{ getPartsTotal() | currency:'INR':'symbol':'1.2-2':'en-IN' }}</span>
            </div>
            <div class="breakdown-item">
              <span>Labour Charges:</span>
              <span>{{ (invoice.labourFee || invoice.LabourFee || 0) | currency:'INR':'symbol':'1.2-2':'en-IN' }}</span>
            </div>
            <div class="breakdown-total">
              <span>Total Amount:</span>
              <span>{{ invoice.totalAmount | currency:'INR':'symbol':'1.2-2':'en-IN' }}</span>
            </div>
          </div>

          <form [formGroup]="paymentForm" (ngSubmit)="processPayment()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Cardholder Name</mat-label>
              <input matInput formControlName="cardHolderName" placeholder="John Doe">
              <mat-error *ngIf="paymentForm.get('cardHolderName')?.hasError('required')">
                Cardholder name is required
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Card Number</mat-label>
              <input 
                matInput 
                formControlName="cardNumber" 
                placeholder="1234567890123456" 
                maxlength="16"
                (input)="formatCardNumber($event)"
                (keypress)="onlyDigits($event)">
              <mat-icon matSuffix>credit_card</mat-icon>
              <mat-error *ngIf="paymentForm.get('cardNumber')?.hasError('required')">
                Card number is required
              </mat-error>
              <mat-error *ngIf="paymentForm.get('cardNumber')?.hasError('pattern')">
                Card number must be exactly 16 digits
              </mat-error>
            </mat-form-field>

            <div class="row">
              <mat-form-field appearance="outline" class="half-width">
                <mat-label>Expiry Date (MM/YY)</mat-label>
                <input 
                  matInput 
                  formControlName="expiryDate" 
                  placeholder="MM/YY" 
                  (input)="formatExpiry($event)" 
                  maxlength="5"
                  (keypress)="onlyDigitsAndSlash($event)">
                <mat-error *ngIf="paymentForm.get('expiryDate')?.hasError('required')">
                  Expiry date is required
                </mat-error>
                <mat-error *ngIf="paymentForm.get('expiryDate')?.hasError('pattern')">
                  Expiry date must be in MM/YY format
                </mat-error>
              </mat-form-field>

              <mat-form-field appearance="outline" class="half-width">
                <mat-label>CVV</mat-label>
                <input 
                  matInput 
                  formControlName="cvv" 
                  placeholder="123" 
                  maxlength="3" 
                  type="password"
                  (keypress)="onlyDigits($event)">
                <mat-error *ngIf="paymentForm.get('cvv')?.hasError('required')">
                  CVV is required
                </mat-error>
                <mat-error *ngIf="paymentForm.get('cvv')?.hasError('pattern')">
                  CVV must be exactly 3 digits
                </mat-error>
              </mat-form-field>
            </div>

            <div class="demo-notice">
              <mat-icon>info</mat-icon>
              <span>Demo payment form (no real transaction)</span>
            </div>

            <div class="actions">
              <button mat-button type="button" routerLink="/service-requests/my-requests">Cancel</button>
              <button mat-raised-button color="primary" type="submit" [disabled]="paymentForm.invalid || isProcessing || !invoice">
                {{ isProcessing ? 'Processing...' : 'Pay Now' }}
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .payment-container {
      display: flex;
      justify-content: center;
      padding: 40px 20px;
      background-color: #f5f5f5;
      min-height: calc(100vh - 64px);
    }
    .payment-card {
      width: 100%;
      max-width: 500px;
      padding: 20px;
    }
    .invoice-breakdown {
      margin-bottom: 24px;
      padding: 16px;
      background-color: #f9f9f9;
      border-radius: 8px;
      border: 1px solid #e0e0e0;
    }
    .breakdown-item {
      display: flex;
      justify-content: space-between;
      padding: 8px 0;
      border-bottom: 1px solid #e0e0e0;
      font-size: 0.95rem;
    }
    .breakdown-item:last-of-type {
      border-bottom: none;
    }
    .breakdown-total {
      display: flex;
      justify-content: space-between;
      padding: 12px 0;
      margin-top: 8px;
      border-top: 2px solid #333;
      font-size: 1.2rem;
      font-weight: 600;
      color: #2e7d32;
    }
    .full-width { width: 100%; margin-bottom: 16px; }
    .row { display: flex; gap: 16px; margin-bottom: 16px; }
    .half-width { flex: 1; }
    .loading-message {
      text-align: center;
      padding: 24px;
      color: #666;
    }
    .actions {
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      margin-top: 24px;
    }
    .demo-notice {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 16px;
      margin-top: 16px;
      background-color: #fff3cd;
      border: 1px solid #ffc107;
      border-radius: 4px;
      color: #856404;
      font-size: 0.875rem;
    }
    .demo-notice mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      color: #856404;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private billingService = inject(BillingService);
  private notification = inject(NotificationService);
  private cdr = inject(ChangeDetectorRef);

  serviceId: number = 0;
  invoice: Invoice | null = null;
  isProcessing = false;

  paymentForm: FormGroup = this.fb.group({
    cardHolderName: ['', Validators.required],
    cardNumber: ['', [Validators.required, Validators.pattern(/^\d{16}$/)]],
    expiryDate: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)]],
    cvv: ['', [Validators.required, Validators.pattern(/^\d{3}$/)]]
  });

  ngOnInit() {
    this.paymentForm.reset({
      cardHolderName: '',
      cardNumber: '',
      expiryDate: '',
      cvv: ''
    });
    
    this.serviceId = Number(this.route.snapshot.paramMap.get('serviceId'));
    if (this.serviceId) {
      this.loadInvoice();
    }
  }

  private loadInvoice() {
    console.log('Loading invoice for serviceId:', this.serviceId);
    
    this.billingService.getInvoiceByServiceId(this.serviceId).subscribe({
      next: (inv) => {
        console.log('Invoice loaded directly:', inv);
        if (inv && inv.invoiceId) {
          this.invoice = inv;
          this.cdr.markForCheck();
        } else {
          console.warn('Invoice response is invalid, trying fallback');
          this.tryFallbackInvoiceLoad();
        }
      },
      error: (err) => {
        console.warn('Direct invoice fetch failed, trying fallback:', err);
        if (err.status === 404) {
          this.tryFallbackInvoiceLoad();
        } else if (err.status === 403 || err.status === 401) {
          this.notification.error('You are not authorized to view this invoice.');
          this.router.navigate(['/service-requests/my-requests']);
        } else {
          this.notification.error('Failed to load invoice. Please try again later.');
          this.router.navigate(['/service-requests/my-requests']);
        }
        this.cdr.markForCheck();
      }
    });
  }

  private tryFallbackInvoiceLoad() {
    this.billingService.getInvoices().subscribe({
      next: (invoices) => {
        console.log('All invoices fetched:', invoices);
        if (!Array.isArray(invoices)) {
          console.error('Invoices response is not an array:', invoices);
          this.notification.error('Invalid response from billing service.');
          this.router.navigate(['/service-requests/my-requests']);
          return;
        }
        
        const found = invoices.find(i => {
          const serviceReqId = i.serviceRequestId || i.ServiceRequestId || 0;
          return String(serviceReqId) === String(this.serviceId);
        });
        
        if (found) {
          console.log('Invoice found in list:', found);
          this.invoice = found;
          this.cdr.markForCheck();
        } else {
          console.error('No invoice found in list for serviceId:', this.serviceId);
          this.notification.error('No invoice found for this service request. The service may not have been approved yet.');
          this.router.navigate(['/service-requests/my-requests']);
        }
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Invoice fallback load error:', err);
        if (err.status === 403 || err.status === 401) {
          this.notification.error('You are not authorized to view billing details.');
        } else {
          this.notification.error('Failed to load invoice details. Please try again later.');
        }
        this.router.navigate(['/service-requests/my-requests']);
        this.cdr.markForCheck();
      }
    });
  }

  onlyDigits(event: KeyboardEvent): boolean {
    const char = String.fromCharCode(event.which);
    if (!/[0-9]/.test(char)) {
      event.preventDefault();
      return false;
    }
    return true;
  }

  onlyDigitsAndSlash(event: KeyboardEvent): boolean {
    const char = String.fromCharCode(event.which);
    if (!/[0-9\/]/.test(char)) {
      event.preventDefault();
      return false;
    }
    return true;
  }

  formatCardNumber(event: Event) {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, ''); // Remove non-digits
    if (value.length > 16) {
      value = value.substring(0, 16); // Enforce max 16 digits
    }
    this.paymentForm.patchValue({ cardNumber: value }, { emitEvent: false });
  }

  formatExpiry(event: Event) {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, ''); // Remove non-digits
    
    if (value.length >= 2) {
      value = value.substring(0, 2) + '/' + value.substring(2, 4);
    }
    
    if (value.length > 5) {
      value = value.substring(0, 5);
    }
    
    this.paymentForm.patchValue({ expiryDate: value }, { emitEvent: false });
  }

  getPartsTotal(): number {
    if (!this.invoice || !this.invoice.partsUsed) {
      return 0;
    }
    return this.invoice.partsUsed.reduce((sum, p) => {
      const pricePerUnit = p.pricePerUnit || p.PricePerUnit || 0;
      const quantity = p.quantity || p.Quantity || 0;
      const totalPrice = p.totalPrice || p.TotalPrice || (pricePerUnit * quantity);
      return sum + (totalPrice || 0);
    }, 0);
  }

  processPayment() {
    if (this.paymentForm.invalid || !this.invoice) {
      if (!this.invoice) {
        this.notification.error('Invoice not loaded. Please refresh the page.');
      }
      return;
    }

    const invoiceId = this.invoice.invoiceId || this.invoice.id || this.invoice.InvoiceId;
    if (!invoiceId) {
      this.notification.error('Invalid invoice. Please try again.');
      return;
    }

    this.isProcessing = true;
    this.cdr.markForCheck();

    this.billingService.payInvoice(invoiceId).subscribe({
      next: () => {
        this.notification.success('Payment successful! Your service is now closed.');
        this.router.navigate(['/service-requests/my-requests']);
      },
      error: (err) => {
        console.error('Payment error:', err);
        this.isProcessing = false;
        const errorMessage = err.error?.message || err.message || 'Payment failed. Please try again.';
        this.notification.error(errorMessage);
        this.cdr.markForCheck();
      }
    });
  }
}
