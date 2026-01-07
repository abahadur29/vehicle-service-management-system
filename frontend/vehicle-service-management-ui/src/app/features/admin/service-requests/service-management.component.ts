import { ChangeDetectionStrategy, Component, inject, OnInit, ViewChild, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { AdminApiService } from '../../../core/services/admin-api.service';
import { ServiceRequestService } from '../../../core/services/service-request.service';
import { NotificationService } from '../../../core/services/notification.service';
import { forkJoin, interval, Subscription, fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { AssignTechnicianDialogComponent, AssignTechnicianDialogData } from './assign-technician-dialog.component';
import { ConfirmCancelDialogComponent, ConfirmCancelDialogData } from './confirm-cancel-dialog.component';
import { ViewDetailsDialogComponent, ViewDetailsDialogData } from './view-details-dialog.component';
import type { ServiceRequest } from '../../../shared/models/service-request.model';
import type { UserManagement } from '../../../shared/models/user-management.model';

@Component({
  standalone: true,
  selector: 'app-service-management',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatIconModule,
    MatTooltipModule,
    MatDialogModule,
    MatCardModule,
    MatFormFieldModule
  ],
  templateUrl: './service-management.component.html',
  styleUrls: ['./service-management.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ServiceManagementComponent implements OnInit, AfterViewInit, OnDestroy {
  private adminApi = inject(AdminApiService);
  private serviceRequestService = inject(ServiceRequestService);
  private notification = inject(NotificationService);
  private dialog = inject(MatDialog);
  private cdr = inject(ChangeDetectorRef);

  private pollingSubscription?: Subscription;
  private tabFocusSubscription?: Subscription;
  private sortChangeSubscription?: Subscription;
  private readonly POLL_INTERVAL = 15000; // Refresh every 15 seconds

  @ViewChild(MatPaginator) set paginator(paginator: MatPaginator) {
    this.dataSource.paginator = paginator;
  }
  @ViewChild(MatSort) set sort(sort: MatSort) {
    if (sort) {
      this.dataSource.sort = sort;
      this.setupSorting();
      if (this.dataSource.data.length > 0 && !this.userHasManuallySorted) {
        this.applyDefaultSort();
      }
    }
  }

  dataSource = new MatTableDataSource<ServiceRequest>([]);
  technicians: UserManagement[] = [];
  displayedColumns: string[] = ['id', 'vehicleModel', 'description', 'status', 'date', 'technicianId', 'actions'];
  private userHasManuallySorted = false;
  private isApplyingDefaultSort = false;

  ngOnInit(): void {
    this.loadData();
    this.startPolling();
    this.setupTabFocusRefresh();
  }

  ngAfterViewInit() {
    if (this.dataSource.sort) {
      this.setupSorting();
      if (!this.userHasManuallySorted && this.dataSource.data.length > 0) {
        this.applyDefaultSort();
      }
      
      this.sortChangeSubscription = this.dataSource.sort.sortChange.subscribe(() => {
        if (this.isApplyingDefaultSort) return;
        
        if (this.dataSource.sort) {
          if (this.dataSource.sort.active !== 'date') {
            this.userHasManuallySorted = true;
          } else if (this.dataSource.sort.direction === 'asc') {
            this.userHasManuallySorted = true;
          }
        }
      });
    }
  }

  private setupSorting() {
    if (!this.dataSource.sort) return;
    
    this.dataSource.sortingDataAccessor = (item: ServiceRequest, property: string) => {
      switch (property) {
        case 'date':
          const dateStr = item.requestedDate || '';
          if (!dateStr) return 0;
          const date = new Date(dateStr);
          const timestamp = date.getTime();
          return isNaN(timestamp) ? 0 : timestamp;
        case 'id':
          return item.id || 0;
        case 'technicianId':
          if (item.status === 'Requested' || !item.technicianName) {
            return '\uffff';
          }
          return (item.technicianName || '').toLowerCase();
        case 'status':
          return (item.status || '').toLowerCase();
        case 'vehicleModel':
          return (item.vehicleModel || '').toLowerCase();
        case 'description':
          return (item.description || '').toLowerCase();
        default:
          return (item as any)[property];
      }
    };
  }

  private applyDefaultSort() {
    if (!this.dataSource.sort) return;
    
    this.isApplyingDefaultSort = true;
    this.dataSource.sort.active = 'date';
    this.dataSource.sort.direction = 'desc';
    this.dataSource.sort.sortChange.emit({
      active: 'date',
      direction: 'desc'
    });
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
    this.cdr.detectChanges();
    this.isApplyingDefaultSort = false;
  }

  ngOnDestroy() {
    this.pollingSubscription?.unsubscribe();
    this.tabFocusSubscription?.unsubscribe();
    this.sortChangeSubscription?.unsubscribe();
  }

  private startPolling() {
    this.pollingSubscription = interval(this.POLL_INTERVAL).subscribe(() => {
      this.loadData();
    });
  }

  private setupTabFocusRefresh() {
    this.tabFocusSubscription = fromEvent(window, 'focus')
      .pipe(
        debounceTime(500),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.loadData();
      });
  }

  loadData() {
    forkJoin({
      services: this.serviceRequestService.getAllRequests(),
      technicians: this.adminApi.getTechnicians(),
    }).subscribe(res => {
      this.technicians = res.technicians;
      const services = res.services.map(s => {
        if (s.technicianId && !s.technicianName) {
          const tech = this.technicians.find(t => t.id === s.technicianId);
          if (tech) {
            s.technicianName = tech.fullName || tech.email;
          }
        }
        return s;
      });
      
      const shouldPreserveSort = this.userHasManuallySorted && this.dataSource.sort;
      const currentSortActive = shouldPreserveSort && this.dataSource.sort ? this.dataSource.sort.active : null;
      const currentSortDirection = shouldPreserveSort && this.dataSource.sort ? this.dataSource.sort.direction : null;
      
      this.dataSource.data = services;
      
      this.dataSource.filterPredicate = (data: ServiceRequest, filter: string) => {
        const normalizedFilter = filter.toLowerCase();
        const serviceId = (data.id || 0).toString();
        const status = (data.status || '').toLowerCase();
        const vehicle = (data.vehicleModel || '').toLowerCase();
        const description = (data.description || '').toLowerCase();
        const technician = (data.technicianName || '').toLowerCase();
        const dateStr = data.requestedDate || '';
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
               technician.includes(normalizedFilter) ||
               dateMatch;
      };
      
      if (this.dataSource.sort) {
        this.setupSorting();
        if (!this.userHasManuallySorted) {
          this.applyDefaultSort();
        } else if (currentSortActive && currentSortDirection) {
          this.isApplyingDefaultSort = true;
          this.dataSource.sort.active = currentSortActive;
          this.dataSource.sort.direction = currentSortDirection;
          this.dataSource.sort.sortChange.emit({
            active: currentSortActive,
            direction: currentSortDirection
          });
          this.isApplyingDefaultSort = false;
        }
      } else {
        this.cdr.detectChanges();
      }
      
      this.cdr.detectChanges();
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  openAssignDialog(service: ServiceRequest): void {
    const dialogData: AssignTechnicianDialogData = {
      serviceId: service.id,
      serviceDescription: service.description,
      vehicleModel: service.vehicleModel,
      technicians: this.technicians
    };

    const dialogRef = this.dialog.open(AssignTechnicianDialogComponent, {
      width: '500px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((result: { technicianId: string; priority: string } | string | undefined) => {
      if (result) {
        if (typeof result === 'object' && result.technicianId) {
          this.assign(service.id, result.technicianId, result.priority);
        } else if (typeof result === 'string') {
          this.assign(service.id, result, 'Normal');
        }
      }
    });
  }

  assign(serviceId: number, technicianId: string, priority: string = 'Normal') {
    if (!technicianId) return;
    
    this.serviceRequestService.assignTechnician({ 
      requestId: serviceId, 
      technicianId, 
      status: 'Assigned',
      priority: priority
    }).subscribe({
      next: () => {
        this.notification.success('Technician assigned successfully');
        this.loadData();
      },
      error: (err) => {
        this.notification.error('Failed to assign technician. Please try again.');
        console.error('Assignment error:', err);
      }
    });
  }

  cancelRequest(service: ServiceRequest) {
    if (service.status === 'Completed' || service.status === 'Closed' || service.status === 'Cancelled') {
      this.notification.error('Cannot cancel a service that is already completed, closed, or cancelled.');
      return;
    }

    const dialogData: ConfirmCancelDialogData = {
      serviceId: service.id,
      vehicleModel: service.vehicleModel,
      description: service.description,
      status: service.status
    };

    const dialogRef = this.dialog.open(ConfirmCancelDialogComponent, {
      width: '500px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.serviceRequestService.assignTechnician({
          requestId: service.id,
          status: 'Cancelled'
        }).subscribe({
          next: () => {
            this.notification.success('Service request cancelled successfully');
            this.loadData();
          },
          error: (err) => {
            const message = err?.error?.message || 'Failed to cancel service request.';
            this.notification.error(message);
          }
        });
      }
    });
  }

  viewDetails(service: ServiceRequest) {
    const dialogData: ViewDetailsDialogData = {
      service: service
    };

    this.dialog.open(ViewDetailsDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: dialogData,
      disableClose: false
    });
  }
}
