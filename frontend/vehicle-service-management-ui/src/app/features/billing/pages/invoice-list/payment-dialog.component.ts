import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

@Component({
  standalone: true,
  selector: 'app-payment-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    ReactiveFormsModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>Payment Details (Dummy Payment)</h2>
    <mat-dialog-content>
      <p class="helper-text">This is a demo payment form. No real transaction will occur.</p>
      <form [formGroup]="paymentForm" class="payment-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Cardholder Name</mat-label>
          <input 
            matInput 
            formControlName="cardHolderName" 
            placeholder="John Doe"
            autocomplete="new-password"
            autocorrect="off"
            autocapitalize="off"
            spellcheck="false">
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
            autocomplete="new-password"
            autocorrect="off"
            autocapitalize="off"
            spellcheck="false"
            inputmode="numeric"
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
              maxlength="5"
              autocomplete="new-password"
              autocorrect="off"
              autocapitalize="off"
              spellcheck="false"
              inputmode="numeric"
              (input)="formatExpiry($event)"
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
              autocomplete="new-password"
              autocorrect="off"
              autocapitalize="off"
              spellcheck="false"
              inputmode="numeric"
              (keypress)="onlyDigits($event)">
            <mat-error *ngIf="paymentForm.get('cvv')?.hasError('required')">
              CVV is required
            </mat-error>
            <mat-error *ngIf="paymentForm.get('cvv')?.hasError('pattern')">
              CVV must be exactly 3 digits
            </mat-error>
          </mat-form-field>
        </div>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" (click)="onPay()" [disabled]="paymentForm.invalid">
        Pay {{ data.amount | currency:'INR':'symbol':'1.2-2':'en-IN' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .helper-text {
      font-size: 0.875rem;
      color: #666;
      margin-bottom: 16px;
      padding: 8px 12px;
      background-color: #f5f5f5;
      border-radius: 4px;
      border-left: 3px solid #ffc107;
    }
    .payment-form { display: flex; flex-direction: column; gap: 10px; padding-top: 10px; }
    .full-width { width: 100%; }
    .row { display: flex; gap: 15px; }
    .half-width { flex: 1; }
  `]
})
export class PaymentDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<PaymentDialogComponent>);
  data = inject(MAT_DIALOG_DATA);

  paymentForm: FormGroup = this.fb.group({
    cardHolderName: ['', Validators.required],
    cardNumber: ['', [Validators.required, Validators.pattern(/^\d{16}$/)]],
    expiryDate: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)]],
    cvv: ['', [Validators.required, Validators.pattern(/^\d{3}$/)]]
  });

  formatCardNumber(event: Event) {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, ''); // Remove non-digits
    if (value.length > 16) {
      value = value.substring(0, 16);
    }
    this.paymentForm.patchValue({ cardNumber: value }, { emitEvent: false });
  }

  onlyDigits(event: KeyboardEvent): boolean {
    const char = String.fromCharCode(event.which || event.keyCode);
    if (!/^\d$/.test(char)) {
      event.preventDefault();
      return false;
    }
    return true;
  }

  onlyDigitsAndSlash(event: KeyboardEvent): boolean {
    const char = String.fromCharCode(event.which || event.keyCode);
    if (!/^[\d\/]$/.test(char)) {
      event.preventDefault();
      return false;
    }
    return true;
  }

  formatExpiry(event: Event) {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, ''); // Remove non-digits
    if (value.length > 2) {
      value = value.substring(0, 2) + '/' + value.substring(2, 4);
    }
    if (value.length > 5) {
      value = value.substring(0, 5);
    }
    this.paymentForm.patchValue({ expiryDate: value }, { emitEvent: false });
  }

  onPay() {
    if (this.paymentForm.valid) {
      this.dialogRef.close(true);
    }
  }
}
