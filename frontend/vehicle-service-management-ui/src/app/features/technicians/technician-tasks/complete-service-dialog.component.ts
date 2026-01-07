import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../core/services/admin-api.service';
import { Part } from '../../../shared/models/part.model';

@Component({
  standalone: true,
  selector: 'app-complete-service-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    FormsModule,
  ],
  template: `
    <h2 mat-dialog-title>Complete Service</h2>
    <mat-dialog-content>
      <mat-form-field class="full-width">
        <mat-label>Remarks</mat-label>
        <textarea matInput [(ngModel)]="remarks" placeholder="Enter completion remarks"></textarea>
      </mat-form-field>

      <h3>Parts Used</h3>
      <div *ngFor="let usage of partsUsed; let i = index" class="part-row">
        <mat-form-field>
          <mat-label>Part</mat-label>
          <mat-select [(ngModel)]="usage.partId">
            <mat-option *ngFor="let part of availableParts" [value]="part.id">
              {{ part.name }} (Stock: {{ part.stockQuantity }})
            </mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field style="width: 80px;">
          <mat-label>Qty</mat-label>
          <input matInput type="number" [(ngModel)]="usage.quantity" min="1">
        </mat-form-field>
        <button mat-icon-button color="warn" (click)="removePart(i)">
          <mat-icon>delete</mat-icon>
        </button>
      </div>
      <button mat-button (click)="addPart()">+ Add Part</button>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" (click)="onConfirm()" [disabled]="!remarks">Complete</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width { width: 100%; }
    .part-row { display: flex; gap: 10px; align-items: center; margin-bottom: 10px; }
  `]
})
export class CompleteServiceDialogComponent implements OnInit {
  private adminApi = inject(AdminApiService);
  private dialogRef = inject(MatDialogRef<CompleteServiceDialogComponent>);
  data = inject(MAT_DIALOG_DATA);

  remarks: string = '';
  partsUsed: { partId: number, quantity: number }[] = [];
  availableParts: Part[] = [];

  ngOnInit() {
    this.adminApi.getParts().subscribe(parts => {
      this.availableParts = parts;
    });
  }

  addPart() {
    this.partsUsed.push({ partId: 0, quantity: 1 });
  }

  removePart(index: number) {
    this.partsUsed.splice(index, 1);
  }

  onConfirm() {
    this.dialogRef.close({
      remarks: this.remarks,
      partsUsed: this.partsUsed.filter(p => p.partId > 0 && p.quantity > 0)
    });
  }
}
