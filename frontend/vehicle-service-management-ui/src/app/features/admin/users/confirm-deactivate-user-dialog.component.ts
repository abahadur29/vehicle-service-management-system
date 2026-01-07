import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDeactivateUserDialogData {
  userName: string;
  userEmail: string;
  isActive: boolean; 
}

@Component({
  standalone: true,
  selector: 'app-confirm-deactivate-user-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon [color]="data.isActive ? 'warn' : 'primary'">{{ data.isActive ? 'warning' : 'check_circle' }}</mat-icon>
      {{ data.isActive ? 'Deactivate User' : 'Activate User' }}
    </h2>
    <mat-dialog-content>
      <div class="dialog-content">
        <p class="confirmation-message">
          This action will change the user's access to the system.
        </p>
        <div class="user-info">
          <div class="info-row">
            <span class="label">Name:</span>
            <span class="value">{{ data.userName }}</span>
          </div>
          <div class="info-row">
            <span class="label">Email:</span>
            <span class="value">{{ data.userEmail }}</span>
          </div>
          <div class="info-row">
            <span class="label">Current Status:</span>
            <span class="value" [class.active]="data.isActive" [class.inactive]="!data.isActive">
              {{ data.isActive ? 'Active' : 'Inactive' }}
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
      color: var(--mat-warn-text-color);
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
    .user-info {
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
    .value.active {
      color: #4caf50;
    }
    .value.inactive {
      color: #f44336;
    }
    .mat-dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--mat-divider-color);
    }
  `]
})
export class ConfirmDeactivateUserDialogComponent {
  private dialogRef = inject(MatDialogRef<ConfirmDeactivateUserDialogComponent>);
  data: ConfirmDeactivateUserDialogData = inject(MAT_DIALOG_DATA);

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}

