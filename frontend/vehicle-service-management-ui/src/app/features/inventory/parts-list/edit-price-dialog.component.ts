import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

export interface EditPriceDialogData {
  partId: number;
  partName: string;
  currentPrice: number;
}

@Component({
  standalone: true,
  selector: 'app-edit-price-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>edit</mat-icon>
      Edit Price: {{ data.partName }}
    </h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="w-100">
          <mat-label>New Unit Price</mat-label>
          <span matPrefix>₹&nbsp;</span>
          <input matInput type="number" formControlName="price" min="0" step="0.01" placeholder="0.00">
          <mat-hint>Current price: {{ data.currentPrice | currency:'INR':'symbol':'1.2-2':'en-IN' }}</mat-hint>
          @if (form.get('price')?.hasError('required') && form.get('price')?.touched) {
            <mat-error>Price is required</mat-error>
          }
          @if (form.get('price')?.hasError('min') && form.get('price')?.touched) {
            <mat-error>Price cannot be negative</mat-error>
          }
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" [disabled]="form.invalid" (click)="onConfirm()">
        Update Price
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .w-100 { 
      width: 100%; 
      margin-top: 8px;
      margin-bottom: 8px;
    }
    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0;
      padding: 24px 24px 16px;
    }
    mat-dialog-content {
      padding: 8px 24px 20px;
      margin: 0;
      min-height: 80px;
    }
    mat-dialog-content form {
      margin-top: 8px;
    }
    mat-form-field {
      width: 100%;
    }
    mat-dialog-actions {
      margin: 0;
      padding: 8px 16px 16px;
      gap: 8px;
    }
    mat-dialog-actions button {
      margin-left: 8px;
    }
    mat-dialog-actions button:first-child {
      margin-left: 0;
    }
  `]
})
export class EditPriceDialogComponent {
  public dialogRef = inject(MatDialogRef<EditPriceDialogComponent>);
  public data: EditPriceDialogData = inject(MAT_DIALOG_DATA);
  private fb = inject(FormBuilder);
  
  form = this.fb.nonNullable.group({
    price: [this.data.currentPrice, [Validators.required, Validators.min(0)]]
  });

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value.price);
    }
  }
}

