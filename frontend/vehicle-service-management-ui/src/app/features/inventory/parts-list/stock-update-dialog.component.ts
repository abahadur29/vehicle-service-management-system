import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  standalone: true,
  selector: 'app-stock-update-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
  template: `
    <h2 mat-dialog-title>Add Stock Quantity: {{ data.name }}</h2>
    <mat-dialog-content>
      <div class="stock-info">
        <p class="current-stock">
          <strong>Current Stock:</strong> {{ currentStock }}
        </p>
      </div>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="w-100">
          <mat-label>Quantity to Add</mat-label>
          <input matInput type="number" formControlName="quantity" min="1" placeholder="Enter quantity to add">
          <mat-error *ngIf="form.get('quantity')?.hasError('required')">Quantity is required</mat-error>
          <mat-error *ngIf="form.get('quantity')?.hasError('min')">Quantity must be greater than zero</mat-error>
        </mat-form-field>
        <div class="final-stock" *ngIf="form.get('quantity')?.value && form.get('quantity')?.valid">
          <p>
            <strong>Final Stock After Update:</strong> 
            <span class="final-value">{{ currentStock + (form.get('quantity')?.value || 0) }}</span>
          </p>
        </div>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancel</button>
      <button mat-raised-button color="primary" [disabled]="form.invalid" (click)="submit()">Add Stock</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .w-100 { 
      width: 100%; 
      margin-bottom: 8px;
    }
    h2[mat-dialog-title] {
      margin: 0;
      padding: 24px 24px 20px;
    }
    mat-dialog-content {
      padding: 0 24px 20px;
      margin: 0;
    }
    .stock-info {
      margin-bottom: 16px;
      padding: 12px;
      background-color: #f5f5f5;
      border-radius: 4px;
    }
    .current-stock {
      margin: 0;
      font-size: 0.95rem;
      color: #333;
    }
    .current-stock strong {
      color: #1976d2;
    }
    .final-stock {
      margin-top: 12px;
      padding: 12px;
      background-color: #e8f5e9;
      border-radius: 4px;
      border-left: 4px solid #4caf50;
    }
    .final-stock p {
      margin: 0;
      font-size: 0.95rem;
      color: #2e7d32;
    }
    .final-stock .final-value {
      font-size: 1.1rem;
      font-weight: 700;
      color: #1b5e20;
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
export class StockUpdateDialogComponent {
  public dialogRef = inject(MatDialogRef<StockUpdateDialogComponent>);
  public data = inject(MAT_DIALOG_DATA);
  private fb = inject(FormBuilder);
  
  currentStock = this.data.stockQuantity || 0;
  
  form = this.fb.group({
    quantity: [0, [Validators.required, Validators.min(1)]]
  });

  submit() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value.quantity);
    }
  }
}
