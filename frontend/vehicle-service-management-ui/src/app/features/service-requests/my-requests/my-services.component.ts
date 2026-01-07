import { ChangeDetectionStrategy, Component, inject, OnInit, ViewChild, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { interval, Subscription, fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ServiceRequestService } from '../../../core/services/service-request.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ServiceRequest } from '../../../shared/models/service-request.model';
import { RescheduleDialogComponent } from './reschedule-dialog.component';
import { ConfirmCancelBookingDialogComponent } from './confirm-cancel-booking-dialog.component';

@Component({
  standalone: true,
  selector: 'app-my-services',
  imports: [
    CommonModule,
    MatTableModule,
    MatChipsModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatDialogModule,
    MatTooltipModule,
    RouterLink
  ],
  templateUrl: './my-services.component.html',
  styleUrls: ['./my-services.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyServicesComponent implements OnInit, AfterViewInit, OnDestroy {
  private serviceRequestService = inject(ServiceRequestService);
  private notification = inject(NotificationService);
  private dialog = inject(MatDialog);
  private route = inject(ActivatedRoute);
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
      if (!this.defaultSortApplied && !this.userHasManuallySorted && this.dataSource.data.length > 0) {
        this.applyDefaultSort();
      }
    }
  }

  dataSource = new MatTableDataSource<ServiceRequest>([]);
  displayedColumns: string[] = ['vehicle', 'serviceId', 'technician', 'description', 'status', 'date', 'actions'];
  private userHasManuallySorted = false;
  private isApplyingDefaultSort = false;
  private defaultSortApplied = false;

  ngOnInit(): void {
    this.loadServices();
    this.startPolling();
    this.setupTabFocusRefresh();
  }

  ngAfterViewInit() {
    if (this.dataSource.sort) {
      this.setupSorting();
      if (!this.defaultSortApplied && !this.userHasManuallySorted) {
        this.applyDefaultSort();
      }
      
      this.sortChangeSubscription = this.dataSource.sort.sortChange.subscribe(() => {
        if (this.isApplyingDefaultSort) return;
        
        this.userHasManuallySorted = true;
        
        if (this.dataSource.paginator) {
          this.dataSource.paginator.firstPage();
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
        case 'serviceId':
          return item.id || 0;
        case 'vehicle':
          return (item.vehicleModel || '').toLowerCase();
        case 'technician':
          const techName = item.technicianName || '';
          return techName.toLowerCase() || '\uffff';
        case 'description':
          return (item.description || '').toLowerCase();
        case 'status':
          return (item.status || '').toLowerCase();
        default:
          return (item as any)[property] || '';
      }
    };
  }

  private applyDefaultSort() {
    if (!this.dataSource.sort || this.userHasManuallySorted) return;
    
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
    this.defaultSortApplied = true;
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
      this.loadServices();
    });
  }

  private setupTabFocusRefresh() {
    this.tabFocusSubscription = fromEvent(window, 'focus')
      .pipe(
        debounceTime(500),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.loadServices();
      });
  }

  loadServices() {
    this.serviceRequestService.getMyBookings().subscribe({
      next: (services) => {
        this.dataSource.data = services;
        
        this.dataSource.filterPredicate = (data: ServiceRequest, filter: string) => {
          const searchStr = filter.toLowerCase();
          const serviceId = (data.id || 0).toString();
          const vehicleMatch = (data.vehicleModel || '').toLowerCase().includes(searchStr);
          const technicianMatch = (data.technicianName || 'Not Assigned').toLowerCase().includes(searchStr);
          const descriptionMatch = (data.description || '').toLowerCase().includes(searchStr);
          const statusMatch = this.getStatusDisplayText(data.status).toLowerCase().includes(searchStr);
          const dateMatch = new Date(data.requestedDate).toLocaleDateString('en-IN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
          }).toLowerCase().includes(searchStr);
          
          return serviceId.includes(searchStr) ||
                 vehicleMatch || 
                 technicianMatch || 
                 descriptionMatch || 
                 statusMatch || 
                 dateMatch;
        };
        
        if (this.dataSource.sort) {
          this.setupSorting();
          if (!this.userHasManuallySorted && !this.defaultSortApplied) {
            this.applyDefaultSort();
          } else if (!this.userHasManuallySorted && this.defaultSortApplied) {
            this.dataSource.sort.active = 'date';
            this.dataSource.sort.direction = 'desc';
            this.cdr.detectChanges();
          }
        } else {
          this.cdr.detectChanges();
        }
        
        const vehicleId = this.route.snapshot.queryParamMap.get('vehicleId');
        if (vehicleId) {
          this.dataSource.filter = vehicleId;
          this.dataSource.filterPredicate = (data: ServiceRequest, filter: string) => {
            return data.vehicleId?.toString() === filter;
          };
        }
      },
      error: (err) => {
        console.error('Failed to load services', err);
        this.notification.error('Failed to load your service requests.');
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

  cancel(id: number) {
    const dialogRef = this.dialog.open(ConfirmCancelBookingDialogComponent, {
      width: '500px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.serviceRequestService.cancelBooking(id).subscribe({
          next: () => {
            this.notification.success('Booking cancelled successfully');
            this.loadServices();
          },
          error: (err) => {
            const message = err?.error?.message || 'Cannot cancel this booking. Service may already be assigned to a technician or in progress.';
            this.notification.error(message);
          }
        });
      }
    });
  }

  canReschedule(service: ServiceRequest): boolean {
    return service.status === 'Requested' && !service.technicianId && !service.technicianName;
  }

  reschedule(service: ServiceRequest) {
    let currentDate = new Date();
    if (service.requestedDate) {
      const dateStr = service.requestedDate;
      const dateMatch = dateStr.match(/^(\d{4})-(\d{2})-(\d{2})/);
      if (dateMatch) {
        const year = parseInt(dateMatch[1]);
        const month = parseInt(dateMatch[2]) - 1;
        const day = parseInt(dateMatch[3]);
        currentDate = new Date(year, month, day);
      } else {
        const isoMatch = dateStr.match(/^(\d{4})-(\d{2})-(\d{2})T/);
        if (isoMatch) {
          const year = parseInt(isoMatch[1]);
          const month = parseInt(isoMatch[2]) - 1;
          const day = parseInt(isoMatch[3]);
          currentDate = new Date(year, month, day);
        }
      }
    }
    
    const dialogRef = this.dialog.open(RescheduleDialogComponent, {
      width: '450px',
      maxWidth: '90vw',
      data: { currentDate: currentDate }
    });

    dialogRef.afterClosed().subscribe(newDate => {
      if (newDate) {
        this.serviceRequestService.rescheduleService(service.id, newDate).subscribe({
          next: () => {
            this.notification.success('Service date updated successfully');
            this.defaultSortApplied = false;
            this.loadServices();
          },
          error: () => this.notification.error('Failed to reschedule service.')
        });
      }
    });
  }

  getActionText(status: string): string {
    if (status === 'Pending Review') {
      return 'Awaiting Approval';
    }
    if (status === 'Assigned' || status === 'In Progress') {
      return 'In Progress';
    }
    return '';
  }

  getStatusDisplayText(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Requested': 'Requested',
      'Assigned': 'Assigned',
      'In Progress': 'In Progress',
      'Pending Review': 'Awaiting Approval',
      'Completed': 'Completed',
      'Closed': 'Paid',
      'Cancelled': 'Cancelled'
    };
    return statusMap[status] || status;
  }
}
