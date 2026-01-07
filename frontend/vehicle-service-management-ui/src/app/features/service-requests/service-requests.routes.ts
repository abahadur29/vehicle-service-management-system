import { Routes } from '@angular/router';
import { MyServicesComponent } from './my-requests/my-services.component';
import { BookServiceComponent } from './book-service/book-service.component';
import { TechnicianTasksComponent } from '../technicians/technician-tasks/technician-tasks.component';
import { TechnicianHistoryComponent } from '../technicians/technician-history/technician-history.component';

export const SERVICE_REQUEST_ROUTES: Routes = [
  { path: 'my-requests', component: MyServicesComponent },
  { path: 'book', component: BookServiceComponent },
  { path: 'book/:vehicleId', component: BookServiceComponent },
  { path: 'my-tasks', component: TechnicianTasksComponent },
  { path: 'history', component: TechnicianHistoryComponent },
];
