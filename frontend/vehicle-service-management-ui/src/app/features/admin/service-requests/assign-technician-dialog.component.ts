import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';

export interface AssignTechnicianDialogData {
  serviceId: number;
  serviceDescription: string;
  vehicleModel: string;
  technicians: Array<{ id: string; fullName?: string; email: string }>;
}

@Component({
  standalone: true,
  selector: 'app-assign-technician-dialog',
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
    <h2 mat-dialog-title>Assign Technician</h2>
    <mat-dialog-content>
      <div class="dialog-content">
        <div class="service-info">
          <p><strong>Service ID:</strong> #{{ data.serviceId }}</p>
          <p><strong>Vehicle:</strong> {{ data.vehicleModel }}</p>
          <p><strong>Description:</strong> {{ data.serviceDescription }}</p>
        </div>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Select Technician</mat-label>
          <mat-select [(ngModel)]="selectedTechnicianId" required>
            <mat-option value="">-- Select a technician --</mat-option>
            @for (tech of data.technicians; track tech.id) {
              <mat-option [value]="tech.id">
                {{ tech.fullName || tech.email }}
              </mat-option>
            }
          </mat-select>
        </mat-form-field>
        
        <div class="priority-section">
          <label class="priority-label">Set Priority:</label>
          <div class="priority-buttons">
            <button mat-raised-button 
                    type="button"
                    [class.selected]="selectedPriority === 'Normal'"
                    [class.normal-btn]="true"
                    (click)="selectedPriority = 'Normal'"
                    class="priority-btn">
              <mat-icon>info</mat-icon>
              Normal
            </button>
            <button mat-raised-button 
                    type="button"
                    [class.selected]="selectedPriority === 'Urgent'"
                    [class.urgent-btn]="true"
                    (click)="selectedPriority = 'Urgent'"
                    class="priority-btn">
              <mat-icon>priority_high</mat-icon>
              Urgent
            </button>
          </div>
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onAssign()" [disabled]="!selectedTechnicianId">
        Assign Technician
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-content {
      min-width: 400px;
      padding: 16px 0;
    }

    .service-info {
      margin-bottom: 24px;
      padding: 16px;
      background-color: #f5f5f5;
      border-radius: 4px;
    }

    .service-info p {
      margin: 8px 0;
      font-size: 14px;
    }

    .service-info strong {
      color: #3f51b5;
    }

    .full-width {
      width: 100%;
    }

    .priority-section {
      margin-top: 24px;
      padding: 16px;
      background-color: #f9f9f9;
      border-radius: 4px;
    }

    .priority-label {
      display: block;
      margin-bottom: 12px;
      font-weight: 500;
      color: #3f51b5;
      font-size: 14px;
    }

    .priority-buttons {
      display: flex;
      gap: 12px;
    }

    .priority-btn {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 12px 16px;
      font-weight: 500;
      transition: all 0.3s ease;
      border: 2px solid transparent;
    }

    .priority-btn.normal-btn {
      background-color: #f5f5f5;
      color: #666;
    }

    .priority-btn.normal-btn.selected {
      background-color: #2196f3;
      color: white;
      border-color: #1976d2;
    }

    .priority-btn.urgent-btn {
      background-color: #f5f5f5;
      color: #666;
    }

    .priority-btn.urgent-btn.selected {
      background-color: #f44336;
      color: white;
      border-color: #d32f2f;
    }

    .priority-btn:hover {
      opacity: 0.9;
      transform: translateY(-1px);
    }

    .priority-btn mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    mat-dialog-actions {
      padding: 16px 24px;
    }
  `]
})
export class AssignTechnicianDialogComponent {
  dialogRef = inject(MatDialogRef<AssignTechnicianDialogComponent>);
  data = inject<AssignTechnicianDialogData>(MAT_DIALOG_DATA);

  selectedTechnicianId: string = '';
  selectedPriority: string = 'Normal';

  onCancel(): void {
    this.dialogRef.close();
  }

  onAssign(): void {
    if (this.selectedTechnicianId) {
      this.dialogRef.close({
        technicianId: this.selectedTechnicianId,
        priority: this.selectedPriority
      });
    }
  }
}

