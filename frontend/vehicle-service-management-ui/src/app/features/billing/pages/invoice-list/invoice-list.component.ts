import { ChangeDetectionStrategy, Component, inject, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute } from '@angular/router';
import { BillingService } from '../../../../core/services/billing.service';
import { Invoice } from '../../../../shared/models/invoice.model';
import { InvoiceDetailDialogComponent } from './invoice-detail-dialog.component';
import { PaymentDialogComponent } from './payment-dialog.component';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  standalone: true,
  selector: 'app-invoice-list',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatDialogModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatTooltipModule
  ],
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvoiceListComponent implements OnInit, AfterViewInit {
  private billingService = inject(BillingService);
  private notification = inject(NotificationService);
  private dialog = inject(MatDialog);
  private route = inject(ActivatedRoute);

  @ViewChild(MatPaginator) set paginator(paginator: MatPaginator) {
    this.dataSource.paginator = paginator;
  }
  @ViewChild(MatSort) set sort(sort: MatSort) {
    this.dataSource.sort = sort;
  }

  dataSource = new MatTableDataSource<Invoice>([]);
  displayedColumns: string[] = ['serviceId', 'amount', 'date', 'status', 'actions'];

  ngOnInit(): void {
    this.dataSource.sortingDataAccessor = (item: Invoice, property: string) => {
      switch (property) {
        case 'date':
          const dateStr = item.issuedDate || item.IssuedDate || '';
          if (!dateStr) return 0;
          const date = new Date(dateStr);
          const timestamp = date.getTime();
          return isNaN(timestamp) ? 0 : timestamp;
        case 'amount':
          const amount = item.totalAmount || item.TotalAmount || 0;
          return typeof amount === 'number' ? amount : Number(amount) || 0;
        case 'status':
          const status = (item.paymentStatus || item.PaymentStatus || '').toString().trim().toLowerCase();
          return status;
        case 'id':
          return item.invoiceId || item.InvoiceId || item.id || 0;
        case 'serviceId':
          return item.serviceRequestId || item.ServiceRequestId || 0;
        default:
          return (item as any)[property];
      }
    };

    this.dataSource.filterPredicate = (data: Invoice, filter: string) => {
      const normalizedFilter = filter.toLowerCase();
      const serviceId = (data.serviceRequestId || data.ServiceRequestId || 0).toString();
      const invoiceId = (data.invoiceId || data.InvoiceId || data.id || 0).toString();
      const status = (data.paymentStatus || data.PaymentStatus || '').toLowerCase();
      const amount = (data.totalAmount || data.TotalAmount || 0).toString();
      const dateStr = data.issuedDate || data.IssuedDate || '';
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
             invoiceId.includes(normalizedFilter) ||
             status.includes(normalizedFilter) ||
             amount.includes(normalizedFilter) ||
             dateMatch;
    };

    this.loadInvoices();
  }

  ngAfterViewInit() {
  }

  loadInvoices() {
    this.billingService.getInvoices().subscribe(invoices => {
      this.dataSource.data = invoices;
      
      const serviceId = this.route.snapshot.queryParamMap.get('serviceId');
      if (serviceId) {
        this.dataSource.filter = serviceId;
      }
      
      if (this.dataSource.sort) {
        this.dataSource.sort.active = 'date';
        this.dataSource.sort.direction = 'desc';
        this.dataSource.sort.sortChange.emit({
          active: 'date',
          direction: 'desc'
        });
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

  view(invoice: Invoice) {
    this.dialog.open(InvoiceDetailDialogComponent, {
      width: '600px',
      data: invoice
    });
  }

  pay(invoice: Invoice) {
    const dialogRef = this.dialog.open(PaymentDialogComponent, {
      width: '400px',
      data: { amount: invoice.totalAmount }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.billingService.payInvoice(invoice.invoiceId).subscribe({
          next: () => {
            this.notification.success('Payment successful! Service closed.');
            this.loadInvoices();
          },
          error: () => this.notification.error('Payment failed.')
        });
      }
    });
  }
}
