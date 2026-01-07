import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmCancelDialogData {
  serviceId: number;
  vehicleModel: string;
  description: string;
  status: string;
}

@Component({
  standalone: true,
  selector: 'app-confirm-cancel-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon color="warn">warning</mat-icon>
      Cancel Service Request
    </h2>
    <mat-dialog-content>
      <div class="dialog-content">
        <p class="confirmation-message">
          Are you sure you want to cancel this service request?
        </p>
        <div class="service-info">
          <div class="info-row">
            <span class="label">Service ID:</span>
            <span class="value">#{{ data.serviceId }}</span>
          </div>
          <div class="info-row">
            <span class="label">Vehicle:</span>
            <span class="value">{{ data.vehicleModel }}</span>
          </div>
          <div class="info-row">
            <span class="label">Current Status:</span>
            <span class="value status-badge" [class]="data.status.toLowerCase().replace(' ', '-')">
              {{ data.status }}
            </span>
          </div>
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="warn" (click)="onConfirm()">
        Confirm Cancellation
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .mat-dialog-title {
      display: flex;
      align-items: center;
      gap: 10px;
    }
    .mat-dialog-content {
      padding: 20px 24px;
      color: var(--mat-secondary-text-color);
    }
    .dialog-content {
      display: flex;
      flex-direction: column;
      gap: 15px;
    }
    .confirmation-message {
      font-size: 1.1em;
      font-weight: 500;
      color: var(--mat-primary-text-color);
      margin-bottom: 10px;
    }
    .service-info {
      background-color: var(--mat-form-field-container-background-color);
      border-radius: 8px;
      padding: 15px;
      border: 1px solid var(--mat-divider-color);
    }
    .info-row {
      display: flex;
      justify-content: space-between;
      padding: 5px 0;
      border-bottom: 1px dashed var(--mat-divider-color);
    }
    .info-row:last-child {
      border-bottom: none;
    }
    .label {
      font-weight: 500;
      color: var(--mat-secondary-text-color);
    }
    .value {
      font-weight: 600;
      color: var(--mat-primary-text-color);
      text-align: right;
      max-width: 60%;
      word-break: break-word;
    }
    .status-badge {
      display: inline-block;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 0.85em;
      font-weight: 700;
      text-transform: uppercase;
    }
    .mat-dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--mat-divider-color);
    }
  `]
})
export class ConfirmCancelDialogComponent {
  private dialogRef = inject(MatDialogRef<ConfirmCancelDialogComponent>);
  data: ConfirmCancelDialogData = inject(MAT_DIALOG_DATA);

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}

