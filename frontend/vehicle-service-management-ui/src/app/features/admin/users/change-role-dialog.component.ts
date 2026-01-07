import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

export interface ChangeRoleDialogData {
  userId: string;
  userName: string;
  userEmail: string;
  currentRole: string;
}

@Component({
  standalone: true,
  selector: 'app-change-role-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatIconModule,
    FormsModule
  ],
  template: `
    <h2 mat-dialog-title>Change User Role</h2>
    <mat-dialog-content>
      <div class="dialog-content">
        <div class="user-info">
          <p><strong>User:</strong> {{ data.userName }}</p>
          <p><strong>Email:</strong> {{ data.userEmail }}</p>
          <p><strong>Current Role:</strong> {{ data.currentRole }}</p>
        </div>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>New Role</mat-label>
          <mat-select [(ngModel)]="selectedRole" required>
            @for (role of availableRoles; track role) {
              <mat-option [value]="role">{{ role }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onConfirm()" [disabled]="!selectedRole || selectedRole === data.currentRole">
        Change Role
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-content {
      min-width: 400px;
      padding: 16px 0;
    }

    .user-info {
      margin-bottom: 24px;
      padding: 16px;
      background-color: #f5f5f5;
      border-radius: 4px;
    }

    .user-info p {
      margin: 8px 0;
      font-size: 14px;
    }

    .user-info strong {
      color: #3f51b5;
    }

    .full-width {
      width: 100%;
    }

    mat-dialog-actions {
      padding: 16px 24px;
    }
  `]
})
export class ChangeRoleDialogComponent implements OnInit {
  dialogRef = inject(MatDialogRef<ChangeRoleDialogComponent>);
  data = inject<ChangeRoleDialogData>(MAT_DIALOG_DATA);

  availableRoles = ['Customer', 'Technician', 'Manager'];
  selectedRole: string = '';

  ngOnInit() {
    if (this.availableRoles.includes(this.data.currentRole)) {
      this.selectedRole = this.data.currentRole;
    } else {
      this.selectedRole = 'Customer';
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.selectedRole && this.selectedRole !== this.data.currentRole) {
      this.dialogRef.close(this.selectedRole);
    }
  }
}

