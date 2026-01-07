import { ChangeDetectionStrategy, Component, inject, OnInit, ViewChild, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { interval, Subscription, fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ServiceRequestService } from '../../../core/services/service-request.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ServiceRequest } from '../../../shared/models/service-request.model';
import { CompleteServiceDialogComponent } from './complete-service-dialog.component';

@Component({
  standalone: true,
  selector: 'app-technician-tasks',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatTooltipModule,
    FormsModule
  ],
  templateUrl: './technician-tasks.component.html',
  styleUrls: ['./technician-tasks.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TechnicianTasksComponent implements OnInit, AfterViewInit, OnDestroy {
  private serviceRequestService = inject(ServiceRequestService);
  private notification = inject(NotificationService);
  private dialog = inject(MatDialog);
  private cdr = inject(ChangeDetectorRef);

  private pollingSubscription?: Subscription;
  private tabFocusSubscription?: Subscription;
  private readonly POLL_INTERVAL = 15000; // 15 seconds

  @ViewChild(MatPaginator) set paginator(paginator: MatPaginator) {
    this.dataSource.paginator = paginator;
  }
  @ViewChild(MatSort) set sort(sort: MatSort) {
    if (sort) {
      this.dataSource.sort = sort;
      this.setupSorting();
      if (!this.userHasManuallySorted && this.dataSource.data.length > 0) {
        this.applyDefaultSort();
      }
      
      this.dataSource.sort.sortChange.subscribe(() => {
        if (this.isApplyingDefaultSort) return;
        this.userHasManuallySorted = true;
        if (this.dataSource.paginator) {
          this.dataSource.paginator.firstPage();
        }
      });
    }
  }

  dataSource = new MatTableDataSource<ServiceRequest>([]);
  displayedColumns: string[] = ['id', 'vehicle', 'description', 'status', 'priority', 'date', 'actions'];
  private userHasManuallySorted = false;
  private isApplyingDefaultSort = false;

  ngOnInit(): void {
    this.dataSource.filterPredicate = (data: ServiceRequest, filter: string) => {
      const normalizedFilter = filter.toLowerCase();
      const serviceId = (data.id || 0).toString();
      const status = (data.status || '').toLowerCase();
      const priority = (data.priority || '').toLowerCase();
      const vehicle = (data.vehicleModel || '').toLowerCase();
      const description = (data.description || '').toLowerCase();
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
             priority.includes(normalizedFilter) ||
             vehicle.includes(normalizedFilter) ||
             description.includes(normalizedFilter) ||
             dateMatch;
    };
    this.loadTasks();
    this.startPolling();
    this.setupTabFocusRefresh();
  }

  ngAfterViewInit() {
  }

  ngOnDestroy() {
    this.pollingSubscription?.unsubscribe();
    this.tabFocusSubscription?.unsubscribe();
  }

  private startPolling() {
    this.pollingSubscription = interval(this.POLL_INTERVAL).subscribe(() => {
      this.loadTasks();
    });
  }

  private setupTabFocusRefresh() {
    this.tabFocusSubscription = fromEvent(window, 'focus')
      .pipe(
        debounceTime(500),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.loadTasks();
      });
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
        case 'priority':
          const priorityOrder: { [key: string]: number } = {
            'urgent': 4,
            'high': 3,
            'normal': 2,
            'low': 1
          };
          return priorityOrder[(item.priority || '').toLowerCase()] || 0;
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
    this.isApplyingDefaultSort = false;
  }

  loadTasks() {
    this.serviceRequestService.getTechnicianTasks().subscribe(tasks => {
      this.dataSource.data = tasks;
      
      if (this.dataSource.sort && !this.userHasManuallySorted) {
        this.setupSorting();
        this.applyDefaultSort();
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

  startService(taskId: number) {
    this.serviceRequestService.assignTechnician({ requestId: taskId, status: 'In Progress' })
      .subscribe(() => {
        this.notification.success('Service started');
        this.loadTasks();
      });
  }

  complete(taskId: number) {
    const dialogRef = this.dialog.open(CompleteServiceDialogComponent, {
      width: '500px',
      data: { taskId }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.serviceRequestService.completeService({
          requestId: taskId,
          remarks: result.remarks,
          partsUsed: result.partsUsed
        }).subscribe({
          next: () => {
            this.notification.success('Service completed successfully!');
            this.loadTasks();
          },
          error: () => this.notification.error('Failed to complete service.')
        });
      }
    });
  }
}
