import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmRoleChangeDialogData {
  userName: string;
  userEmail: string;
  currentRole: string;
  newRole: string;
}

@Component({
  standalone: true,
  selector: 'app-confirm-role-change-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>warning</mat-icon>
      Confirm Role Change
    </h2>
    <mat-dialog-content>
      <div class="dialog-content">
        <p class="confirmation-message">
          Are you sure you want to change this user's role?
        </p>
        <div class="user-info">
          <div class="info-row">
            <span class="label">User Name:</span>
            <span class="value">{{ data.userName }}</span>
          </div>
          <div class="info-row">
            <span class="label">Email:</span>
            <span class="value">{{ data.userEmail }}</span>
          </div>
          <div class="info-row">
            <span class="label">Current Role:</span>
            <span class="value role-badge current">{{ data.currentRole }}</span>
          </div>
          <div class="info-row">
            <span class="label">New Role:</span>
            <span class="value role-badge new">{{ data.newRole }}</span>
          </div>
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onConfirm()">
        Confirm Change
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 12px;
      margin: 0;
      padding: 24px 24px 16px 24px;
      color: #1e293b;
    }

    h2[mat-dialog-title] mat-icon {
      color: #f59e0b;
      font-size: 28px;
      width: 28px;
      height: 28px;
    }

    mat-dialog-content {
      padding: 0 24px 24px 24px;
      min-width: 400px;
    }

    .dialog-content {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .confirmation-message {
      font-size: 16px;
      font-weight: 500;
      color: #1e293b;
      margin: 0;
      padding: 16px;
      background-color: #fef3c7;
      border-left: 4px solid #f59e0b;
      border-radius: 4px;
    }

    .user-info {
      display: flex;
      flex-direction: column;
      gap: 12px;
      padding: 16px;
      background-color: #f8fafc;
      border-radius: 4px;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 8px 0;
      border-bottom: 1px solid #e2e8f0;
    }

    .info-row:last-child {
      border-bottom: none;
    }

    .label {
      font-weight: 600;
      color: #64748b;
      font-size: 14px;
    }

    .value {
      color: #1e293b;
      font-size: 14px;
      text-align: right;
    }

    .role-badge {
      display: inline-block;
      padding: 4px 12px;
      border-radius: 12px;
      font-weight: 600;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .role-badge.current {
      background-color: #e2e8f0;
      color: #475569;
    }

    .role-badge.new {
      background-color: #dbeafe;
      color: #1e40af;
    }

    mat-dialog-actions {
      padding: 16px 24px;
      margin: 0;
      border-top: 1px solid #e2e8f0;
    }

    button[mat-button] {
      margin-right: 8px;
    }

    @media (max-width: 600px) {
      mat-dialog-content {
        min-width: auto;
      }

      .info-row {
        flex-direction: column;
        align-items: flex-start;
        gap: 4px;
      }

      .value {
        text-align: left;
      }
    }
  `]
})
export class ConfirmRoleChangeDialogComponent {
  dialogRef = inject(MatDialogRef<ConfirmRoleChangeDialogComponent>);
  data = inject<ConfirmRoleChangeDialogData>(MAT_DIALOG_DATA);

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}

