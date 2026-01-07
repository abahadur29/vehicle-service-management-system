import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { VehicleService } from '../../../../core/services/vehicle.service';
import { Vehicle } from '../../../../shared/models/vehicle.model';

@Component({
  standalone: true,
  selector: 'app-vehicle-selection-dialog',
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
    <h2 mat-dialog-title>{{ dialogTitle }}</h2>
    <mat-dialog-content>
      <p>{{ dialogMessage }}</p>
      @if (isLoading) {
        <div class="loading-state">
          <mat-icon>hourglass_empty</mat-icon>
          <p>Loading vehicles...</p>
        </div>
      } @else if (hasError) {
        <div class="error-state">
          <mat-icon color="warn">error</mat-icon>
          <p>Failed to load vehicles. Please try again.</p>
        </div>
      } @else if (vehicles.length === 0) {
        <div class="no-vehicles">
          <mat-icon color="warn">warning</mat-icon>
          <p>No vehicles found. Please add a vehicle first.</p>
        </div>
      } @else {
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Vehicle</mat-label>
          <mat-select [(ngModel)]="selectedVehicleId">
            @for (vehicle of vehicles; track vehicle.id) {
              <mat-option [value]="vehicle.id">
                <mat-icon>directions_car</mat-icon>
                {{ vehicle.make }} {{ vehicle.model }} ({{ vehicle.licensePlate }})
              </mat-option>
            }
          </mat-select>
        </mat-form-field>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" (click)="onConfirm()" [disabled]="isLoading || hasError || !selectedVehicleId || vehicles.length === 0">
        {{ confirmButtonText }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      min-width: 300px;
    }
    mat-dialog-content {
      padding: 20px;
      min-height: 150px;
    }
    p {
      margin-bottom: 16px;
      color: #666;
    }
    .loading-state,
    .error-state,
    .no-vehicles {
      text-align: center;
      padding: 20px;
      color: #666;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
    }
    .loading-state mat-icon,
    .error-state mat-icon,
    .no-vehicles mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
      animation: pulse 2s ease-in-out infinite;
    }
    .loading-state mat-icon {
      animation: spin 1s linear infinite;
    }
    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }
    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.5; }
    }
  `]
})
export class VehicleSelectionDialogComponent implements OnInit {
  private vehicleService = inject(VehicleService);
  private dialogRef = inject(MatDialogRef<VehicleSelectionDialogComponent>);
  private cdr = inject(ChangeDetectorRef);
  data = inject(MAT_DIALOG_DATA, { optional: true });

  vehicles: Vehicle[] = [];
  selectedVehicleId: number | null = null;
  isLoading = true;
  hasError = false;
  dialogTitle = 'Select Vehicle';
  dialogMessage = 'Please select a vehicle:';
  confirmButtonText = 'Continue';

  ngOnInit() {
    if (this.data) {
      this.dialogTitle = this.data.title || 'Select Vehicle';
      this.dialogMessage = this.data.message || 'Please select a vehicle:';
      this.confirmButtonText = this.data.confirmButtonText || 'Continue';
    }

    Promise.resolve().then(() => {
      this.vehicleService.getVehicles().subscribe({
        next: (vehicles) => {
          this.vehicles = vehicles || [];
          this.isLoading = false;

          if (this.vehicles.length === 1) {
            this.selectedVehicleId = this.vehicles[0].id;
          }
          // Update UI after data loads
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Failed to load vehicles:', err);
          this.hasError = true;
          this.isLoading = false;
          this.vehicles = [];
          // Update UI after error
          this.cdr.detectChanges();
        }
      });
    });
  }

  onConfirm() {
    if (this.selectedVehicleId) {
      this.dialogRef.close(this.selectedVehicleId);
    }
  }
}

