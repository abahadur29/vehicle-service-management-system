import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { ServiceRequestService } from '../../../core/services/service-request.service';
import { ServiceRequest } from '../../../shared/models/service-request.model';

@Component({
  standalone: true,
  selector: 'app-technician-history',
  imports: [
    CommonModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule
  ],
  templateUrl: './technician-history.component.html',
  styleUrls: ['./technician-history.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TechnicianHistoryComponent implements OnInit, AfterViewInit {
  private serviceRequestService = inject(ServiceRequestService);
  private cdr = inject(ChangeDetectorRef);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<ServiceRequest>([]);
  displayedColumns: string[] = ['id', 'vehicle', 'description', 'status', 'completionDate'];

  ngOnInit(): void {
    this.dataSource.sortingDataAccessor = (item: ServiceRequest, property: string) => {
      switch (property) {
        case 'completionDate':
          const dateStr = item.completionDate || item.requestedDate || '';
          if (!dateStr) return 0;
          const date = new Date(dateStr);
          const timestamp = date.getTime();
          return isNaN(timestamp) ? 0 : timestamp;
        case 'id':
          return item.id || 0;
        default:
          return (item as any)[property] || '';
      }
    };

    this.dataSource.filterPredicate = (data: ServiceRequest, filter: string) => {
      const normalizedFilter = filter.toLowerCase();
      const serviceId = (data.id || 0).toString();
      const status = (data.status || '').toLowerCase();
      const vehicle = (data.vehicleModel || '').toLowerCase();
      const description = (data.description || '').toLowerCase();
      const dateStr = data.completionDate || data.requestedDate || '';
      let dateMatch = false;
      if (dateStr) {
        const date = new Date(dateStr);
        if (!isNaN(date.getTime())) {
          const day = date.getDate().toString().padStart(2, '0');
          const month = (date.getMonth() + 1).toString().padStart(2, '0');
          const year = date.getFullYear().toString();
          const ddMMyyyy = `${day}/${month}/${year}`;
          const dMyyyy = `${date.getDate()}/${month}/${year}`;
          const ddMyyyy = `${day}/${date.getMonth() + 1}/${year}`;
          const formattedDate = date.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric'
          }).toLowerCase();
          const shortDate = date.toLocaleDateString('en-US', {
            month: 'numeric',
            day: 'numeric',
            year: 'numeric'
          }).toLowerCase();
          const longDate = date.toLocaleDateString('en-US', {
            month: 'long',
            day: 'numeric',
            year: 'numeric'
          }).toLowerCase();
          dateMatch = ddMMyyyy.includes(normalizedFilter) ||
                     dMyyyy.includes(normalizedFilter) ||
                     ddMyyyy.includes(normalizedFilter) ||
                     formattedDate.includes(normalizedFilter) ||
                     shortDate.includes(normalizedFilter) ||
                     longDate.includes(normalizedFilter);
        }
      }
      
      return serviceId.includes(normalizedFilter) ||
             status.includes(normalizedFilter) ||
             vehicle.includes(normalizedFilter) ||
             description.includes(normalizedFilter) ||
             dateMatch;
    };
    
    this.loadHistory();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    
    if (this.sort) {
      this.sort.active = 'completionDate';
      this.sort.direction = 'desc';
      this.sort.sortChange.emit({ active: 'completionDate', direction: 'desc' });
    }
    
    this.cdr.detectChanges();
  }

  loadHistory() {
    this.serviceRequestService.getTechnicianServiceHistory().subscribe({
      next: (history) => {
        this.dataSource.data = history;
        
        if (this.sort && this.dataSource.data.length > 0) {
          this.sort.active = 'completionDate';
          this.sort.direction = 'desc';
          this.sort.sortChange.emit({ active: 'completionDate', direction: 'desc' });
        }
        
        if (this.paginator) {
          this.paginator.firstPage();
        }
        
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading service history:', err);
        this.dataSource.data = [];
        this.cdr.detectChanges();
      }
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }
}

