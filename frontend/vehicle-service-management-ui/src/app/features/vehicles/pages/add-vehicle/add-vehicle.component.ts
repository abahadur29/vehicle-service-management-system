import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { VehicleService } from '../../../../core/services/vehicle.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { finalize } from 'rxjs';

@Component({
  standalone: true,
  selector: 'app-add-vehicle',
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, RouterLink],
  templateUrl: './add-vehicle.component.html',
  styleUrls: ['./add-vehicle.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddVehicleComponent {
  private fb = inject(FormBuilder);
  private vehicleService = inject(VehicleService);
  private notification = inject(NotificationService);
  private router = inject(Router);

  isSubmitting = false;

  form = this.fb.group({
    licensePlate: ['', [Validators.required]],
    make: ['', [Validators.required]],
    model: ['', [Validators.required]],
    year: [new Date().getFullYear(), [Validators.required, Validators.min(1900), Validators.max(new Date().getFullYear() + 1)]],
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = {
      licensePlate: (this.form.value.licensePlate?.toUpperCase() || ''),
      make: this.form.value.make || '',
      model: this.form.value.model || '',
      year: this.form.value.year || new Date().getFullYear()
    };
    
    this.vehicleService.addVehicle(formValue)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: () => {
          this.notification.success('Vehicle added successfully!');
          this.router.navigate(['/vehicles']);
        },
        error: (err) => {
          this.notification.error('Failed to add vehicle. Please check your inputs.');
          console.error('Error adding vehicle', err);
        }
      });
  }

  onLicensePlateInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const upperValue = input.value.toUpperCase();
    if (input.value !== upperValue) {
      this.form.patchValue({ licensePlate: upperValue }, { emitEvent: false });
      input.value = upperValue;
    }
  }
}
