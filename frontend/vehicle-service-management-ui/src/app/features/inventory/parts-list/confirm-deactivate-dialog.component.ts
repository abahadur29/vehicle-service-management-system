import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDeactivateDialogData {
  partName: string;
  isActive: boolean;
}

@Component({
  standalone: true,
  selector: 'app-confirm-deactivate-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>{{ data.isActive ? 'block' : 'check_circle' }}</mat-icon>
      {{ data.isActive ? 'Deactivate' : 'Activate' }} Part
    </h2>
    <mat-dialog-content>
      <div class="dialog-content">
        <p class="confirmation-message">
          Are you sure you want to {{ data.isActive ? 'deactivate' : 'activate' }} this part?
        </p>
        <div class="part-info">
          <div class="info-row">
            <span class="label">Part Name:</span>
            <span class="value">{{ data.partName }}</span>
          </div>
          <div class="info-row">
            <span class="label">Action:</span>
            <span class="value action-badge" [class.deactivate]="data.isActive" [class.activate]="!data.isActive">
              {{ data.isActive ? 'Deactivate' : 'Activate' }}
            </span>
          </div>
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button [color]="data.isActive ? 'warn' : 'primary'" (click)="onConfirm()">
        {{ data.isActive ? 'Deactivate' : 'Activate' }}
      </button>
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
    .part-info {
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
    }
    .action-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 0.85em;
      font-weight: 700;
      color: white;
    }
    .action-badge.deactivate {
      background-color: #dc2626;
    }
    .action-badge.activate {
      background-color: #16a34a;
    }
    .mat-dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--mat-divider-color);
    }
  `]
})
export class ConfirmDeactivateDialogComponent {
  private dialogRef = inject(MatDialogRef<ConfirmDeactivateDialogComponent>);
  data: ConfirmDeactivateDialogData = inject(MAT_DIALOG_DATA);

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}

