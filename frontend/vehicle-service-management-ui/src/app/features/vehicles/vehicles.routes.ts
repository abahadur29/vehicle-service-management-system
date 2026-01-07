import { Routes } from '@angular/router';
import { VehicleListComponent } from './pages/vehicle-list/vehicle-list.component';
import { AddVehicleComponent } from './pages/add-vehicle/add-vehicle.component';

export const VEHICLE_ROUTES: Routes = [
  { path: '', component: VehicleListComponent },
  { path: 'add', component: AddVehicleComponent },
];
