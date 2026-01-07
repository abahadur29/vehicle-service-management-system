import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Notification } from '../../../../../shared/models/notification.model';

@Component({
  standalone: true,
  selector: 'app-notifications-dialog',
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>notifications</mat-icon>
      <span>Notifications</span>
      @if (data.length > 0) {
        <span class="notification-count-badge">{{ data.length }}</span>
      }
    </h2>
    <mat-dialog-content>
      @if (data.length === 0) {
        <div class="no-notifications">
          <mat-icon>notifications_off</mat-icon>
          <p class="no-notifications-title">No new notifications</p>
          <p class="no-notifications-subtitle">You're all caught up!</p>
        </div>
      } @else {
        <div class="notification-list">
          @for (note of data; track note.id) {
            <div class="notification-item">
              <div class="notification-content">
                <div class="notification-message">{{ note.message }}</div>
                <div class="notification-meta">
                  @if (note.serviceRequestId) {
                    <span class="service-id">Service #{{ note.serviceRequestId }}</span>
                  }
                  <span class="notification-time">{{ formatTimestamp(note.changedOn) }}</span>
                </div>
              </div>
            </div>
          }
        </div>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .mat-dialog-title {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 20px 24px 16px;
      border-bottom: 1px solid var(--mat-divider-color);
      margin: 0;
    }
    .mat-dialog-title mat-icon {
      color: var(--mat-primary-color);
    }
    .notification-count-badge {
      background: var(--mat-primary-color);
      color: white;
      border-radius: 12px;
      padding: 2px 8px;
      font-size: 0.75rem;
      font-weight: 600;
      margin-left: auto;
    }
    .no-notifications {
      text-align: center;
      padding: 60px 20px;
      color: var(--mat-secondary-text-color);
    }
    .no-notifications mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
      opacity: 0.3;
      color: var(--mat-secondary-text-color);
    }
    .no-notifications-title {
      font-size: 1.125rem;
      font-weight: 600;
      color: var(--mat-primary-text-color);
      margin: 0 0 8px 0;
    }
    .no-notifications-subtitle {
      font-size: 0.875rem;
      color: var(--mat-secondary-text-color);
      margin: 0;
    }
    .notification-list {
      padding: 0;
    }
    .notification-item {
      padding: 16px 24px;
      border-bottom: 1px solid var(--mat-divider-color);
      transition: background-color 0.2s ease;
    }
    .notification-item:last-child {
      border-bottom: none;
    }
    .notification-item:hover {
      background-color: var(--mat-hover-state-layer-color, rgba(0, 0, 0, 0.04));
    }
    .notification-content {
      width: 100%;
    }
    .notification-message {
      font-size: 0.9375rem;
      font-weight: 400;
      line-height: 1.5;
      color: var(--mat-primary-text-color);
      margin-bottom: 6px;
      word-wrap: break-word;
      overflow-wrap: break-word;
      white-space: normal;
    }
    .notification-meta {
      display: flex;
      align-items: center;
      gap: 12px;
      flex-wrap: wrap;
      margin-top: 4px;
    }
    .service-id {
      font-size: 0.75rem;
      font-weight: 500;
      color: var(--mat-primary-color);
      padding: 2px 6px;
      background-color: rgba(var(--mat-primary-color-rgb, 63, 81, 181), 0.1);
      border-radius: 4px;
    }
    .notification-time {
      font-size: 0.75rem;
      color: var(--mat-secondary-text-color);
      font-weight: 400;
    }
    mat-dialog-content {
      padding: 0;
      max-height: 60vh;
      overflow-y: auto;
      overflow-x: hidden;
    }
    
    mat-dialog-content::-webkit-scrollbar {
      width: 8px;
    }
    
    mat-dialog-content::-webkit-scrollbar-track {
      background: var(--mat-divider-color);
    }
    
    mat-dialog-content::-webkit-scrollbar-thumb {
      background: var(--mat-primary-color);
      border-radius: 4px;
    }
    mat-dialog-actions {
      padding: 12px 24px;
      border-top: 1px solid var(--mat-divider-color);
      margin: 0;
    }
  `]
})
export class NotificationsDialogComponent {
  data: Notification[] = inject(MAT_DIALOG_DATA);

  formatTimestamp(dateString: string): string {
    let utcDateString = dateString.trim();
    
    const hasTimezone = utcDateString.endsWith('Z') || 
                        /[+-]\d{2}:\d{2}$/.test(utcDateString);
    
    if (!hasTimezone && utcDateString.length > 0) {
      utcDateString = utcDateString + 'Z';
    }
    
    const date = new Date(utcDateString);
    const now = new Date();
    
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);
    if (diffMins < 1) {
      return 'Just now';
    } else if (diffMins < 60) {
      return `${diffMins} ${diffMins === 1 ? 'minute' : 'minutes'} ago`;
    } else if (diffHours < 24) {
      return `${diffHours} ${diffHours === 1 ? 'hour' : 'hours'} ago`;
    } else if (diffDays < 7) {
      return `${diffDays} ${diffDays === 1 ? 'day' : 'days'} ago`;
    } else {
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined });
    }
  }

}
