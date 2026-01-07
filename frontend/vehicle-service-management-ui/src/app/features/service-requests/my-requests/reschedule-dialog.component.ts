import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, DateAdapter } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { CustomDateAdapter } from '../../../shared/adapters/custom-date-adapter';
import { DATE_FORMAT_PROVIDERS } from '../../../shared/adapters/date-format';

export interface RescheduleDialogData {
  currentDate?: Date;
}

@Component({
  standalone: true,
  selector: 'app-reschedule-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule
  ],
  providers: [
    { provide: DateAdapter, useClass: CustomDateAdapter },
    ...DATE_FORMAT_PROVIDERS
  ],
  template: `
    <h2 mat-dialog-title>Reschedule Service</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="reschedule-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>New Requested Date</mat-label>
          <input 
            matInput 
            [matDatepicker]="picker" 
            formControlName="newDate" 
            [min]="minDate">
          <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
          <mat-datepicker #picker (closed)="onDatePickerClosed()"></mat-datepicker>
          <mat-error *ngIf="form.get('newDate')?.hasError('required')">Date is required</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancel</button>
      <button mat-raised-button color="primary" [disabled]="form.invalid" (click)="submit()">Reschedule</button>
    </mat-dialog-actions>
  `,
  styles: [`
    :host {
      display: block;
    }

    h2[mat-dialog-title] {
      margin: 0;
      padding: 24px 24px 20px;
      font-size: 1.5rem;
      font-weight: 500;
      line-height: 1.2;
      color: rgba(0, 0, 0, 0.87);
    }

    mat-dialog-content {
      min-width: 400px;
      padding: 0 24px 16px;
      margin: 0;
      overflow: visible;
    }

    .reschedule-form {
      display: flex;
      flex-direction: column;
      padding-top: 4px;
    }

    .full-width {
      width: 100%;
      margin-bottom: 0;
    }

    mat-form-field {
      width: 100%;
      margin-bottom: 0;
    }

    mat-form-field ::ng-deep {
      .mat-mdc-text-field-wrapper {
        padding-bottom: 0;
      }
      
      .mat-mdc-form-field-subscript-wrapper {
        margin-top: 4px;
      }
      
      input[readonly] {
        cursor: pointer;
      }
    }

    mat-dialog-actions {
      padding: 8px 24px 24px;
      margin: 0;
      border-top: none;
      min-height: 52px;
    }

    mat-dialog-actions button {
      margin-left: 8px;
    }

    mat-dialog-actions button:first-child {
      margin-left: 0;
    }
  `]
})
export class RescheduleDialogComponent {
  public dialogRef = inject(MatDialogRef<RescheduleDialogComponent>);
  private fb = inject(FormBuilder);
  data = inject<RescheduleDialogData>(MAT_DIALOG_DATA, { optional: true });
  
  minDate = new Date();
  form = this.fb.group({
    newDate: [this.getInitialDate(), [Validators.required]]
  });

  private getInitialDate(): Date {
    return this.data?.currentDate || new Date();
  }

  onDatePickerClosed() {
    this.form.get('newDate')?.updateValueAndValidity();
  }

  submit() {
    if (this.form.valid) {
      const selectedDate = this.form.value.newDate;
      if (selectedDate instanceof Date) {
        const dateAtNoon = new Date(
          selectedDate.getFullYear(),
          selectedDate.getMonth(),
          selectedDate.getDate(),
          12,
          0,
          0
        );
        this.dialogRef.close(dateAtNoon.toISOString());
      } else if (typeof selectedDate === 'string') {
        const date = new Date(selectedDate);
        if (!isNaN(date.getTime())) {
          const dateAtNoon = new Date(
            date.getFullYear(),
            date.getMonth(),
            date.getDate(),
            12,
            0,
            0
          );
          this.dialogRef.close(dateAtNoon.toISOString());
        } else {
          this.dialogRef.close(selectedDate);
        }
      }
    }
  }
}
