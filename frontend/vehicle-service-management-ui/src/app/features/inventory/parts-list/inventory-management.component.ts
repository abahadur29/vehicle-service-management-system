import { ChangeDetectionStrategy, Component, inject, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCardModule } from '@angular/material/card';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { AdminApiService } from '../../../core/services/admin-api.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TokenService } from '../../../core/auth/services/token.service';
import { StockUpdateDialogComponent } from './stock-update-dialog.component';
import { EditPriceDialogComponent, EditPriceDialogData } from './edit-price-dialog.component';
import { ConfirmDeactivateDialogComponent, ConfirmDeactivateDialogData } from './confirm-deactivate-dialog.component';
import type { Part } from '../../../shared/models/part.model';

@Component({
  standalone: true,
  selector: 'app-inventory-management',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatTooltipModule,
    MatCardModule,
    FormsModule,
    MatDialogModule
  ],
  templateUrl: './inventory-management.component.html',
  styleUrls: ['./inventory-management.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InventoryManagementComponent implements OnInit, AfterViewInit {
  private api = inject(AdminApiService);
  private notification = inject(NotificationService);
  private dialog = inject(MatDialog);
  private token = inject(TokenService);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<Part>([]);
  displayedColumns: string[] = ['name', 'stockQuantity', 'price', 'actions'];
  
  role = (this.token.getRole() ?? '').toLowerCase();
  isAdmin = this.role === 'admin';
  isManager = this.role === 'manager';
  isTechnician = this.role === 'technician';
  
  get columnsToDisplay(): string[] {
    if (this.isTechnician) {
      return ['name', 'stockQuantity'];
    }
    return this.displayedColumns;
  }

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    
    this.dataSource.sortingDataAccessor = (data: Part, sortHeaderId: string) => {
      switch (sortHeaderId) {
        case 'stockQuantity':
          return data.stockQuantity ?? 0;
        case 'price':
          return data.price ?? data.unitPrice ?? 0;
        case 'name':
          return (data.name || '').toLowerCase();
        default:
          return '';
      }
    };
    
    this.dataSource.sortData = (data: Part[], sort: Sort): Part[] => {
      if (!sort.active || sort.direction === '') {
        return data;
      }

      return data.sort((a, b) => {
        const isAsc = sort.direction === 'asc';
        let valueA: number | string;
        let valueB: number | string;

        switch (sort.active) {
          case 'stockQuantity':
            valueA = a.stockQuantity ?? 0;
            valueB = b.stockQuantity ?? 0;
            return (valueA < valueB ? -1 : valueA > valueB ? 1 : 0) * (isAsc ? 1 : -1);
          case 'price':
            valueA = a.price ?? a.unitPrice ?? 0;
            valueB = b.price ?? b.unitPrice ?? 0;
            return (valueA < valueB ? -1 : valueA > valueB ? 1 : 0) * (isAsc ? 1 : -1);
          case 'name':
            valueA = (a.name || '').toLowerCase();
            valueB = (b.name || '').toLowerCase();
            return valueA < valueB ? -1 : valueA > valueB ? 1 : 0;
          default:
            return 0;
        }
      });
    };
  }

  load() {
    this.api.getParts().subscribe({
      next: (parts) => {
        this.dataSource.data = parts;
        
        setTimeout(() => {
          if (this.sort && !this.sort.active) {
            this.sort.sort({ id: 'stockQuantity', start: 'asc', disableClear: false });
          }
        }, 0);
      },
      error: () => this.notification.error('Failed to load inventory.')
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  updateStock(part: Part) {
    const dialogRef = this.dialog.open(StockUpdateDialogComponent, {
      width: '400px',
      data: part
    });

    dialogRef.afterClosed().subscribe(quantityToAdd => {
      if (quantityToAdd !== undefined && quantityToAdd > 0) {
        this.api.updateStock(part.id, quantityToAdd).subscribe({
          next: () => {
            this.notification.success(`Added ${quantityToAdd} unit(s) to stock successfully`);
            this.load();
          },
          error: () => this.notification.error('Failed to add stock.')
        });
      }
    });
  }

  updatePrice(part: Part) {
    const dialogData: EditPriceDialogData = {
      partId: part.id,
      partName: part.name,
      currentPrice: part.price || part.unitPrice || 0
    };

    const dialogRef = this.dialog.open(EditPriceDialogComponent, {
      width: '500px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((newPrice: number | undefined) => {
      if (newPrice !== undefined && newPrice !== null) {
        this.api.updatePrice(part.id, newPrice).subscribe({
          next: () => {
            this.notification.success('Price updated successfully');
            this.load();
          },
          error: () => this.notification.error('Failed to update price.')
        });
      }
    });
  }

  toggleActive(part: Part) {
    const dialogData: ConfirmDeactivateDialogData = {
      partName: part.name,
      isActive: part.isActive ?? true
    };

    const dialogRef = this.dialog.open(ConfirmDeactivateDialogComponent, {
      width: '500px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        const action = part.isActive ? 'deactivate' : 'activate';
        this.api.toggleActive(part.id).subscribe({
          next: () => {
            this.notification.success(`Part ${action}d successfully`);
            this.load();
          },
          error: () => this.notification.error(`Failed to ${action} part.`)
        });
      }
    });
  }
}
