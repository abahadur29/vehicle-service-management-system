import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { Invoice } from '../../../../shared/models/invoice.model';

@Component({
  standalone: true,
  selector: 'app-invoice-detail-dialog',
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatTableModule],
  template: `
    <h2 mat-dialog-title>Invoice #{{ data.invoiceId }}</h2>
    <mat-dialog-content>
      <p><strong>Date:</strong> {{ data.issuedDate | date }}</p>
      <p><strong>Status:</strong> {{ data.paymentStatus }}</p>
      
      <h3>Parts Used</h3>
      <table mat-table [dataSource]="data.partsUsed" class="mat-elevation-z1">
        <ng-container matColumnDef="name">
          <th mat-header-cell *matHeaderCellDef> Part </th>
          <td mat-cell *matCellDef="let part"> {{ part.partName }} </td>
        </ng-container>
        <ng-container matColumnDef="qty">
          <th mat-header-cell *matHeaderCellDef> Qty </th>
          <td mat-cell *matCellDef="let part"> {{ part.quantity }} </td>
        </ng-container>
        <ng-container matColumnDef="price">
          <th mat-header-cell *matHeaderCellDef> Price </th>
          <td mat-cell *matCellDef="let part"> {{ (part.pricePerUnit || part.PricePerUnit || 0) | currency:'INR':'symbol':'1.2-2':'en-IN' }} </td>
        </ng-container>
        <ng-container matColumnDef="total">
          <th mat-header-cell *matHeaderCellDef> Total </th>
          <td mat-cell *matCellDef="let part"> {{ getPartTotal(part) | currency:'INR':'symbol':'1.2-2':'en-IN' }} </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="['name', 'qty', 'price', 'total']"></tr>
        <tr mat-row *matRowDef="let row; columns: ['name', 'qty', 'price', 'total'];"></tr>
      </table>

      <div style="margin-top: 20px; text-align: right;">
        <div style="margin-bottom: 8px;">
          <strong>Parts Total:</strong> 
          {{ getPartsTotal() | currency:'INR':'symbol':'1.2-2':'en-IN' }}
        </div>
        <div style="margin-bottom: 8px;">
          <strong>Labour Charges:</strong> 
          {{ (data.labourFee || data.LabourFee || 0) | currency:'INR':'symbol':'1.2-2':'en-IN' }}
        </div>
        <h2 style="margin-top: 12px; border-top: 2px solid #333; padding-top: 8px;">
          Total: {{ data.totalAmount | currency:'INR':'symbol':'1.2-2':'en-IN' }}
        </h2>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    table { width: 100%; }
    h3 { margin-top: 20px; }
  `]
})
export class InvoiceDetailDialogComponent {
  data: Invoice = inject(MAT_DIALOG_DATA);

  getPartTotal(part: any): number {
    const quantity = part.quantity || part.Quantity || 0;
    const pricePerUnit = part.pricePerUnit || part.PricePerUnit || 0;
    return quantity * pricePerUnit;
  }

  getPartsTotal(): number {
    const parts = this.data.partsUsed || [];
    return parts.reduce((sum, p) => {
      return sum + this.getPartTotal(p);
    }, 0);
  }
}
