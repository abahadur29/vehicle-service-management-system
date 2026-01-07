import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDeleteDialogData {
  userName: string;
  userEmail: string;
  itemType?: string; 
}

@Component({
  standalone: true,
  selector: 'app-confirm-delete-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon color="warn">warning</mat-icon>
      Confirm Deletion
    </h2>
    <mat-dialog-content>
      <div class="dialog-content">
        <p class="confirmation-message">
          Are you sure you want to delete this {{ data.itemType || 'item' }}? This action cannot be undone.
        </p>
        <div class="user-info">
          @if (data.userName) {
            <div class="info-row">
              <span class="label">Name:</span>
              <span class="value">{{ data.userName }}</span>
            </div>
          }
          @if (data.userEmail) {
            <div class="info-row">
              <span class="label">Email:</span>
              <span class="value">{{ data.userEmail }}</span>
            </div>
          }
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="warn" (click)="onConfirm()">Delete</button>
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
    .mat-dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--mat-divider-color);
    }
  `]
})
export class ConfirmDeleteDialogComponent {
  private dialogRef = inject(MatDialogRef<ConfirmDeleteDialogComponent>);
  data: ConfirmDeleteDialogData = inject(MAT_DIALOG_DATA);

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}

