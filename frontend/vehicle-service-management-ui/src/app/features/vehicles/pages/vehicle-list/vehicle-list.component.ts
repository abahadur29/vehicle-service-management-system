import { ChangeDetectionStrategy, Component, inject, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { RouterLink } from '@angular/router';
import { VehicleService } from '../../../../core/services/vehicle.service';
import { Vehicle } from '../../../../shared/models/vehicle.model';

@Component({
  standalone: true,
  selector: 'app-vehicle-list',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    RouterLink
  ],
  templateUrl: './vehicle-list.component.html',
  styleUrls: ['./vehicle-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VehicleListComponent implements OnInit, AfterViewInit {
  private vehicleService = inject(VehicleService);

  @ViewChild(MatPaginator) set paginator(paginator: MatPaginator) {
    this.dataSource.paginator = paginator;
  }
  @ViewChild(MatSort) set sort(sort: MatSort) {
    this.dataSource.sort = sort;
  }

  dataSource = new MatTableDataSource<Vehicle>([]);
  displayedColumns: string[] = ['licensePlate', 'make', 'model', 'year', 'actions'];

  ngOnInit(): void {
    this.loadVehicles();
  }

  ngAfterViewInit() {
    // Handled by setters
  }

  loadVehicles() {
    this.vehicleService.getVehicles().subscribe(vehicles => {
      this.dataSource.data = vehicles;
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
