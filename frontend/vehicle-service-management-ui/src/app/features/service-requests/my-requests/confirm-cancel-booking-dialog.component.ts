import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  standalone: true,
  selector: 'app-confirm-cancel-booking-dialog',
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
      margin: 0;
    }
    .mat-dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--mat-divider-color);
    }
  `]
})
export class ConfirmCancelBookingDialogComponent {
  private dialogRef = inject(MatDialogRef<ConfirmCancelBookingDialogComponent>);

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}

