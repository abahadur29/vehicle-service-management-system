import { ChangeDetectionStrategy, Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { Router, RouterLink } from '@angular/router';
import { DashboardService } from '../../../../core/services/dashboard.service';
import { TokenService } from '../../../../core/auth/services/token.service';
import { VehicleService } from '../../../../core/services/vehicle.service';
import { ServiceRequestService } from '../../../../core/services/service-request.service';
import { Observable, forkJoin } from 'rxjs';
import { ServiceRequest } from '../../../../shared/models/service-request.model';
import { VehicleSelectionDialogComponent } from './vehicle-selection-dialog.component';

@Component({
  standalone: true,
  selector: 'app-dashboard-home',
  imports: [
    CommonModule,
    MatCardModule,
    MatGridListModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    RouterLink
  ],
  templateUrl: './dashboard-home.component.html',
  styleUrls: ['./dashboard-home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardHomeComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private tokenService = inject(TokenService);
  private dialog = inject(MatDialog);
  private router = inject(Router);
  private vehicleService = inject(VehicleService);
  private serviceRequestService = inject(ServiceRequestService);
  private cdr = inject(ChangeDetectorRef);

  role = (this.tokenService.getRole() ?? '').toLowerCase();
  fullName = localStorage.getItem('vsm_fullname');
  today = new Date();

  stats$: Observable<any> | undefined;
  workloadColumns = ['technician', 'activeTasks'];

  technicianTasks: ServiceRequest[] = [];
  technicianHistory: ServiceRequest[] = [];
  totalAssigned = 0;
  inProgress = 0;
  completed = 0;
  todayTasks: ServiceRequest[] = [];

  ngOnInit(): void {
    if (this.role === 'admin' || this.role === 'manager') {
      this.stats$ = forkJoin({
        workload: this.dashboardService.getTechnicianWorkload(),
      });
    } else if (this.role === 'technician') {
      this.loadTechnicianData();
    }
  }

  private loadTechnicianData() {
    forkJoin({
      tasks: this.serviceRequestService.getTechnicianTasks(),
      history: this.serviceRequestService.getTechnicianServiceHistory()
    }).subscribe({
      next: (data) => {
        this.technicianTasks = data.tasks || [];
        this.technicianHistory = data.history || [];
        
        this.totalAssigned = this.technicianTasks.filter(t => t.status === 'Assigned').length;
        this.inProgress = this.technicianTasks.filter(t => t.status === 'In Progress').length;
        this.completed = this.technicianHistory.length;
        
        const allActiveTasks = this.technicianTasks.filter(t => 
          t.status === 'Assigned' || t.status === 'In Progress'
        );
        this.todayTasks = allActiveTasks.slice(0, 3);
        
        this.cdr.detectChanges();
      },
      error: () => {
        this.technicianTasks = [];
        this.technicianHistory = [];
        this.cdr.detectChanges();
      }
    });
  }

  openServiceHistory() {
    const dialogRef = this.dialog.open(VehicleSelectionDialogComponent, {
      width: '400px',
      data: { 
        title: 'Select Vehicle', 
        message: 'Please select a vehicle to view its service history:',
        confirmButtonText: 'View History'
      }
    });

    dialogRef.afterClosed().subscribe(vehicleId => {
      if (vehicleId) {
        this.router.navigate(['/service-requests/my-requests'], {
          queryParams: { vehicleId }
        });
      }
    });
  }

  openBookService() {
    const dialogRef = this.dialog.open(VehicleSelectionDialogComponent, {
      width: '400px',
      data: { 
        title: 'Select Vehicle', 
        message: 'Please select a vehicle to book a service:',
        confirmButtonText: 'Book Service'
      }
    });

    dialogRef.afterClosed().subscribe(vehicleId => {
      if (vehicleId) {
        this.router.navigate(['/service-requests/book', vehicleId]);
      } else {
        this.vehicleService.getVehicles().subscribe(vehicles => {
          if (vehicles.length === 0) {
            this.router.navigate(['/vehicles/add']);
          }
        });
      }
    });
  }
}
