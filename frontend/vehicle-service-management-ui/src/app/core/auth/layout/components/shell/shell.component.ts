import { ChangeDetectionStrategy, Component, computed, inject, signal, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { interval, Subscription } from 'rxjs';

import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { RouterLink } from '@angular/router';


import { TokenService } from '../../../services/token.service';
import { AuthService } from '../../../services/auth.service';
import { SidenavComponent } from '../sidenav/sidenav.component';
import { ServiceRequestService } from '../../../../services/service-request.service';
import { AdminApiService } from '../../../../services/admin-api.service';
import { NotificationsDialogComponent } from './notifications-dialog.component';
import { Notification } from '../../../../../shared/models/notification.model';


@Component({
  standalone: true,
  selector: 'app-shell',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    MatSidenavModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatMenuModule,
    MatBadgeModule,
    SidenavComponent,
  ],
  templateUrl: './shell.component.html',
  styleUrls: ['./shell.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent implements OnInit, OnDestroy {
  private breakpointObserver = inject(BreakpointObserver);
  private token = inject(TokenService);
  private auth = inject(AuthService);
  private router = inject(Router);
  private serviceRequestService = inject(ServiceRequestService);
  private adminApi = inject(AdminApiService);
  private dialog = inject(MatDialog);
  private cdr = inject(ChangeDetectorRef);

  private isHandsetSig = signal(false);
  lowStockCount = signal(0);
  unreadNotificationCount = signal(0);
  private notificationPollingSubscription?: Subscription;
  private readonly NOTIFICATION_POLL_INTERVAL = 30000;
  private readonly READ_NOTIFICATIONS_KEY = 'vsm_read_notifications';

  role = computed(() => this.token.getRole() ?? 'User');
  fullName = signal(localStorage.getItem('vsm_fullname') || 'User');

  constructor() {
    this.breakpointObserver.observe([Breakpoints.Handset]).subscribe((res) => {
      this.isHandsetSig.set(res.matches);
    });

    const currentRole = this.token.getRole()?.toLowerCase();
    if (currentRole === 'admin' || currentRole === 'manager') {
      this.adminApi.getLowStock().subscribe(parts => {
        this.lowStockCount.set(parts.length);
      });
    }
  }

  ngOnInit() {
    this.checkUnreadNotifications();
    this.notificationPollingSubscription = interval(this.NOTIFICATION_POLL_INTERVAL).subscribe(() => {
      this.checkUnreadNotifications();
    });
  }

  ngOnDestroy() {
    this.notificationPollingSubscription?.unsubscribe();
  }

  isHandset = computed(() => this.isHandsetSig());

  private getReadNotificationIds(): Set<number> {
    const stored = localStorage.getItem(this.READ_NOTIFICATIONS_KEY);
    if (!stored) return new Set();
    try {
      const ids = JSON.parse(stored) as number[];
      return new Set(ids);
    } catch {
      return new Set();
    }
  }

  private saveReadNotificationIds(ids: Set<number>) {
    localStorage.setItem(this.READ_NOTIFICATIONS_KEY, JSON.stringify(Array.from(ids)));
  }

  private checkUnreadNotifications() {
    if (!this.token.getToken()) return;

    this.serviceRequestService.getNotifications().subscribe({
      next: (notifications: Notification[]) => {
        const filteredNotifications = this.filterNotificationsByRole(notifications);
        const readIds = this.getReadNotificationIds();
        const unreadCount = filteredNotifications.filter(n => !readIds.has(n.id)).length;
        this.unreadNotificationCount.set(unreadCount);
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Failed to check notifications', err);
        this.unreadNotificationCount.set(0);
        this.cdr.markForCheck();
      }
    });
  }

  showNotifications() {
    if (!this.token.getToken()) {
      this.router.navigate(['/auth/login']);
      return;
    }
    this.serviceRequestService.getNotifications().subscribe({
      next: (notes: Notification[]) => {
        const filteredNotes = this.filterNotificationsByRole(notes);
        
        const readIds = this.getReadNotificationIds();
        filteredNotes.forEach(note => readIds.add(note.id));
        this.saveReadNotificationIds(readIds);
        this.unreadNotificationCount.set(0);
        this.cdr.markForCheck();

        const dialogRef = this.dialog.open(NotificationsDialogComponent, {
          width: '500px',
          maxWidth: '90vw',
          maxHeight: '80vh',
          data: filteredNotes || []
        });

        dialogRef.afterClosed().subscribe(() => {
          this.checkUnreadNotifications();
        });
      },
      error: (err) => {
        console.error('Failed to fetch notifications', err);
        this.dialog.open(NotificationsDialogComponent, {
          width: '500px',
          data: []
        });
      }
    });
  }

  private filterNotificationsByRole(notes: Notification[]): Notification[] {
    const currentRole = this.token.getRole()?.toLowerCase();
    
    if (currentRole === 'manager') {
      return notes.filter(note => {
        const action = (note.action || '').toLowerCase();
        const message = (note.message || '').toLowerCase();
        
        if (action === 'added') {
          if (message.includes('service request') && 
              (message.includes('created') || 
               message.includes('has been created') || 
               message.includes('has been successfully booked') ||
               message.includes('new service request'))) {
            return true;
          }
          return true;
        }
        
        if (message.includes('new service request') && message.includes('has been created')) {
          return true;
        }
        
        if (action === 'paymentreceived' || 
            message.includes('payment received') || 
            message.includes('payment completed')) {
          return true;
        }
        
        if (action === 'updated' && message.includes('technician assigned')) {
          return true;
        }
        
        if (action === 'status changed' || (action === 'updated' && message.includes('status'))) {
          if (message.includes('started') || 
              message.includes('in progress') || 
              (message.includes('completed') && !message.includes('payment'))) {
            return false;
          }
        }
        
        if (action === 'technician assigned' && !message.includes('status changed')) {
          return false;
        }
        
        return true;
      });
    }
    
    return notes;
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }
}