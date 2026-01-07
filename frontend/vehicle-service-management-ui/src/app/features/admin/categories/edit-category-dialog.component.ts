import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { ServiceCategory } from '../../../shared/models/service-request.model';

export interface EditCategoryDialogData {
  category: ServiceCategory;
}

@Component({
  standalone: true,
  selector: 'app-edit-category-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>Edit Category</h2>
    <mat-dialog-content>
      <form [formGroup]="editForm" class="edit-category-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Category Name</mat-label>
          <input matInput formControlName="name" required>
          @if (editForm.controls.name.invalid && editForm.controls.name.touched) {
            <mat-error>Category name is required (min 2 characters)</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Base Price</mat-label>
          <input matInput type="number" formControlName="basePrice" required>
          <span matPrefix>₹&nbsp;</span>
          @if (editForm.controls.basePrice.invalid && editForm.controls.basePrice.touched) {
            <mat-error>Base price is required (must be >= 0)</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="3"></textarea>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Status</mat-label>
          <mat-select formControlName="isActive">
            <mat-option [value]="true">Active</mat-option>
            <mat-option [value]="false">Inactive</mat-option>
          </mat-select>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onSave()" [disabled]="editForm.invalid">
        Update Category
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .edit-category-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      min-width: 400px;
      padding-top: 1rem;
    }

    .full-width {
      width: 100%;
    }

    mat-dialog-content {
      padding: 0 24px;
    }

    mat-dialog-actions {
      padding: 16px 24px;
      margin: 0;
    }
  `]
})
export class EditCategoryDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<EditCategoryDialogComponent>);
  data = inject<EditCategoryDialogData>(MAT_DIALOG_DATA);

  editForm = this.fb.nonNullable.group({
    id: [this.data.category.id],
    name: [this.data.category.name || '', [Validators.required, Validators.minLength(2)]],
    basePrice: [this.data.category.basePrice || 0, [Validators.required, Validators.min(0)]],
    description: [this.data.category.description || ''],
    isActive: [this.data.category.isActive !== false],
  });

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.dialogRef.close(this.editForm.getRawValue());
  }
}

