import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, DateAdapter } from '@angular/material/core';
import { ServiceRequestService } from '../../../core/services/service-request.service';
import { VehicleService } from '../../../core/services/vehicle.service';
import { AdminApiService } from '../../../core/services/admin-api.service';
import { NotificationService } from '../../../core/services/notification.service';
import { CreateServiceRequest } from '../../../shared/models/service-request.model';
import { CustomDateAdapter } from '../../../shared/adapters/custom-date-adapter';
import { DATE_FORMAT_PROVIDERS } from '../../../shared/adapters/date-format';
import { finalize } from 'rxjs';

@Component({
  standalone: true,
  selector: 'app-book-service',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  providers: [
    { provide: DateAdapter, useClass: CustomDateAdapter },
    ...DATE_FORMAT_PROVIDERS
  ],
  templateUrl: './book-service.component.html',
  styleUrls: ['./book-service.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookServiceComponent implements OnInit {
  private fb = inject(FormBuilder);
  private serviceRequestService = inject(ServiceRequestService);
  private vehicleService = inject(VehicleService);
  private adminApi = inject(AdminApiService);
  private notification = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  isSubmitting = false;
  private isSubmittingGuard = false;
  minDate = new Date();
  vehicles$ = this.vehicleService.getVehicles();
  categories$ = this.adminApi.getCategories(true);

  form = this.fb.group({
    vehicleId: [null as number | null, [Validators.required]],
    serviceCategoryId: [null as number | null, [Validators.required]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    requestedDate: [new Date(), [Validators.required]],
  });

  ngOnInit(): void {
    const vehicleId = this.route.snapshot.params['vehicleId'];
    if (vehicleId) {
      this.form.patchValue({ vehicleId: +vehicleId });
    }
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.isSubmitting || this.isSubmittingGuard) {
      return;
    }

    const formValue = this.form.value;
    let requestedDateValue: string;
    if (formValue.requestedDate instanceof Date) {
      const selectedDate = formValue.requestedDate;
      const dateAtNoon = new Date(
        selectedDate.getFullYear(),
        selectedDate.getMonth(),
        selectedDate.getDate(),
        12,
        0,
        0
      );
      requestedDateValue = dateAtNoon.toISOString();
    } else if (typeof formValue.requestedDate === 'string') {
      const date = new Date(formValue.requestedDate);
      if (!isNaN(date.getTime())) {
        const dateAtNoon = new Date(
          date.getFullYear(),
          date.getMonth(),
          date.getDate(),
          12,
          0,
          0
        );
        requestedDateValue = dateAtNoon.toISOString();
      } else {
        const now = new Date();
        const dateAtNoon = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 12, 0, 0);
        requestedDateValue = dateAtNoon.toISOString();
      }
    } else {
      const now = new Date();
      const dateAtNoon = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 12, 0, 0);
      requestedDateValue = dateAtNoon.toISOString();
    }
    
    const serviceRequest: CreateServiceRequest = {
      vehicleId: formValue.vehicleId!,
      serviceCategoryId: formValue.serviceCategoryId!,
      description: formValue.description!,
      priority: 'Normal',
      requestedDate: requestedDateValue,
    };

    this.isSubmitting = true;
    this.isSubmittingGuard = true;
    this.serviceRequestService.bookService(serviceRequest)
      .pipe(finalize(() => {
        this.isSubmitting = false;
        this.isSubmittingGuard = false;
      }))
      .subscribe({
        next: () => {
          this.notification.success('Service booked successfully!');
          this.router.navigate(['/service-requests/my-requests']);
        },
        error: (err) => {
          this.notification.error('Failed to book service. Please try again.');
          console.error('Error booking service', err);
        }
      });
  }

  onCancel() {
    this.router.navigate(['/vehicles']);
  }
}
