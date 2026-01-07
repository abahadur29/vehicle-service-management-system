import { ChangeDetectionStrategy, Component, inject, OnInit, ViewChild, AfterViewInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminApiService } from '../../../core/services/admin-api.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ServiceCategory } from '../../../shared/models/service-request.model';
import { EditCategoryDialogComponent, EditCategoryDialogData } from './edit-category-dialog.component';

@Component({
  standalone: true,
  selector: 'app-category-management',
  imports: [
    CommonModule,
    MatTableModule,
    MatCardModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule,
    ReactiveFormsModule,
    MatTooltipModule,
    MatDialogModule,
  ],
  templateUrl: './category-management.component.html',
  styleUrls: ['./category-management.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryManagementComponent implements OnInit, AfterViewInit {
  private adminApi = inject(AdminApiService);
  private notification = inject(NotificationService);
  private fb = inject(FormBuilder);
  private dialog = inject(MatDialog);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<ServiceCategory>([]);
  displayedColumns: string[] = ['name', 'price', 'status', 'actions'];
  showCreateForm = signal(false);

  categoryForm = this.fb.nonNullable.group({
    id: [0],
    name: ['', [Validators.required, Validators.minLength(2)]],
    basePrice: [0, [Validators.required, Validators.min(0)]],
    description: [''],
    isActive: [true],
  });

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    
    this.dataSource.sortingDataAccessor = (item: ServiceCategory, property: string) => {
      switch (property) {
        case 'price':
          return item.basePrice || 0;
        case 'status':
          return item.isActive !== false ? 0 : 1;
        default:
          return (item as any)[property];
      }
    };
    
    // Custom filter predicate to handle status column
    this.dataSource.filterPredicate = (data: ServiceCategory, filter: string) => {
      const searchStr = filter.toLowerCase();
      const name = (data.name || '').toLowerCase();
      const price = (data.basePrice || 0).toString();
      const status = data.isActive !== false ? 'active' : 'inactive';
      
      return name.includes(searchStr) ||
             price.includes(searchStr) ||
             status.includes(searchStr);
    };
    
    if (this.dataSource.sort) {
      this.dataSource.sort.active = 'name';
      this.dataSource.sort.direction = 'asc';
    }
  }

  load() {
    this.adminApi.getCategories().subscribe({
      next: (cats) => {
        this.dataSource.data = cats;
        if (this.dataSource.sort && !this.dataSource.sort.active) {
          this.dataSource.sort.active = 'name';
          this.dataSource.sort.direction = 'asc';
        }
      },
      error: () => this.notification.error('Failed to load categories.')
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  startCreate() {
    this.categoryForm.reset();
    this.categoryForm.patchValue({ id: 0, isActive: true });
    this.showCreateForm.set(true);
  }

  startEdit(category: ServiceCategory) {
    const dialogRef = this.dialog.open(EditCategoryDialogComponent, {
      width: '500px',
      data: { category } as EditCategoryDialogData,
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminApi.manageCategory(result).subscribe({
          next: () => {
            this.notification.success('Category updated successfully');
            this.load();
          },
          error: (err) => {
            const message = err?.error?.message || 'Failed to update category.';
            this.notification.error(message);
          }
        });
      }
    });
  }

  cancelEdit() {
    this.showCreateForm.set(false);
    this.categoryForm.reset();
  }

  saveCategory() {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const dto = this.categoryForm.getRawValue();
    this.adminApi.manageCategory(dto).subscribe({
      next: () => {
        this.notification.success('Category created successfully');
        this.cancelEdit();
        this.load();
      },
      error: (err) => {
        const message = err?.error?.message || 'Failed to save category.';
        this.notification.error(message);
      }
    });
  }
}
