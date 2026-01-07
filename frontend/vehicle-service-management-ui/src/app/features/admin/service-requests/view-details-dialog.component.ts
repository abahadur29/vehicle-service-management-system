import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import type { ServiceRequest } from '../../../shared/models/service-request.model';

export interface ViewDetailsDialogData {
  service: ServiceRequest;
}

@Component({
  standalone: true,
  selector: 'app-view-details-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>info</mat-icon>
      Service Request Details
    </h2>
    <mat-dialog-content>
      <div class="details-container">
        <div class="detail-section">
          <h3 class="section-title">
            <mat-icon>tag</mat-icon>
            Request Information
          </h3>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">Service ID:</span>
              <span class="detail-value">#{{ data.service.id }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Status:</span>
              <span class="detail-value">
                <span class="status-badge" [class]="data.service.status.toLowerCase().replace(' ', '-')">
                  {{ data.service.status }}
                </span>
              </span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Priority:</span>
              <span class="detail-value">
                <span class="priority-badge" [class.urgent]="data.service.priority === 'Urgent'" [class.normal]="data.service.priority === 'Normal'">
                  {{ data.service.priority }}
                </span>
              </span>
            </div>
            <div class="detail-item full-width">
              <span class="detail-label">Description:</span>
              <span class="detail-value">{{ data.service.description || 'No description provided' }}</span>
            </div>
          </div>
        </div>

        <div class="detail-section">
          <h3 class="section-title">
            <mat-icon>directions_car</mat-icon>
            Vehicle Information
          </h3>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">Vehicle Model:</span>
              <span class="detail-value">{{ data.service.vehicleModel }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Vehicle ID:</span>
              <span class="detail-value">#{{ data.service.vehicleId }}</span>
            </div>
          </div>
        </div>

        <div class="detail-section" *ngIf="data.service.technicianName || data.service.technicianId">
          <h3 class="section-title">
            <mat-icon>person</mat-icon>
            Technician Information
          </h3>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">Assigned Technician:</span>
              <span class="detail-value">{{ data.service.technicianName || 'Not assigned' }}</span>
            </div>
            <div class="detail-item" *ngIf="data.service.technicianId">
              <span class="detail-label">Technician ID:</span>
              <span class="detail-value">{{ data.service.technicianId }}</span>
            </div>
          </div>
        </div>

        <div class="detail-section">
          <h3 class="section-title">
            <mat-icon>schedule</mat-icon>
            Timeline
          </h3>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">Requested Date:</span>
              <span class="detail-value">{{ data.service.requestedDate | date:'dd/MM/yyyy' }}</span>
            </div>
          </div>
        </div>

        <div class="detail-section" *ngIf="data.service.invoiceId">
          <h3 class="section-title">
            <mat-icon>receipt</mat-icon>
            Billing Information
          </h3>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">Invoice ID:</span>
              <span class="detail-value">#{{ data.service.invoiceId }}</span>
            </div>
          </div>
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onClose()">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .mat-dialog-title {
      display: flex;
      align-items: center;
      gap: 10px;
      color: var(--mat-primary-text-color);
    }
    .mat-dialog-content {
      padding: 20px 24px;
      max-height: 70vh;
      overflow-y: auto;
    }
    .details-container {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }
    .detail-section {
      background-color: var(--mat-form-field-container-background-color);
      border-radius: 8px;
      padding: 16px;
      border: 1px solid var(--mat-divider-color);
    }
    .section-title {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 1.1em;
      font-weight: 600;
      color: var(--mat-primary-text-color);
      margin: 0 0 12px 0;
      padding-bottom: 8px;
      border-bottom: 2px solid var(--mat-divider-color);
    }
    .section-title mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }
    .detail-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 12px;
    }
    .detail-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .detail-item.full-width {
      grid-column: 1 / -1;
    }
    .detail-label {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--mat-secondary-text-color);
    }
    .detail-value {
      font-size: 1rem;
      font-weight: 600;
      color: var(--mat-primary-text-color);
      word-break: break-word;
    }
    .status-badge, .priority-badge {
      display: inline-block;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 0.85em;
      font-weight: 700;
      text-transform: uppercase;
    }
    .status-badge {
      background-color: var(--mat-form-field-container-background-color);
      color: var(--mat-primary-text-color);
    }
    .priority-badge.urgent {
      background-color: #fef2f2;
      color: #dc2626;
    }
    .priority-badge.normal {
      background-color: #eff6ff;
      color: #2563eb;
    }
    .mat-dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--mat-divider-color);
    }
  `]
})
export class ViewDetailsDialogComponent {
  private dialogRef = inject(MatDialogRef<ViewDetailsDialogComponent>);
  data: ViewDetailsDialogData = inject(MAT_DIALOG_DATA);

  onClose(): void {
    this.dialogRef.close();
  }
}

