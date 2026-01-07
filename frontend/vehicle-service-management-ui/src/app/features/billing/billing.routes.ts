import { Routes } from '@angular/router';
import { InvoiceListComponent } from './pages/invoice-list/invoice-list.component';
import { PaymentComponent } from './pages/payment/payment.component';

export const BILLING_ROUTES: Routes = [
  { path: '', component: InvoiceListComponent },
  { path: 'payment/:serviceId', component: PaymentComponent },
];
