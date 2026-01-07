import { ChangeDetectionStrategy, Component, inject, signal, ViewChild, AfterViewInit, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

import { AdminApiService } from '../../../core/services/admin-api.service';
import { TokenService } from '../../../core/auth/services/token.service';
import { NotificationService } from '../../../core/services/notification.service';
import type { UserManagement } from '../../../shared/models/user-management.model';
import { ChangeRoleDialogComponent, ChangeRoleDialogData } from './change-role-dialog.component';
import { ConfirmRoleChangeDialogComponent, ConfirmRoleChangeDialogData } from './confirm-role-change-dialog.component';
import { ConfirmDeactivateUserDialogComponent, ConfirmDeactivateUserDialogData } from './confirm-deactivate-user-dialog.component';

@Component({
  standalone: true,
  selector: 'app-user-management',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatDialogModule,
    MatIconModule,
    MatTooltipModule,
  ],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserManagementComponent implements OnInit, AfterViewInit {
  private api = inject(AdminApiService);
  private token = inject(TokenService);
  private notification = inject(NotificationService);
  private fb = inject(FormBuilder);
  private dialog = inject(MatDialog);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<UserManagement>([]);
  users = signal<UserManagement[]>([]);
  myUserId = this.token.getUserId();

  columns = ['name', 'email', 'role', 'actions'];

  createUserRoles = ['Customer', 'Technician', 'Manager'];

  createUserForm = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    phoneNumber: ['', Validators.required],
    role: ['Customer', Validators.required],
  });

  showCreateForm = signal(false);

  isAdminUser(user: UserManagement): boolean {
    return user.role?.toLowerCase() === 'admin';
  }

  canChangeRole(user: UserManagement): boolean {
    return user.id !== this.myUserId && !this.isAdminUser(user);
  }

  ngOnInit() {
    this.load();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    
    this.dataSource.sortingDataAccessor = (data: UserManagement, sortHeaderId: string) => {
      switch (sortHeaderId) {
        case 'name':
          return data.fullName || 'N/A';
        case 'email':
          return data.email || '';
        case 'role':
          return data.role || '';
        default:
          return '';
      }
    };
    
    this.dataSource.sortData = (data: UserManagement[], sort: Sort): UserManagement[] => {
      if (!sort.active || sort.direction === '') {
        return data;
      }

      return data.sort((a, b) => {
        const isAsc = sort.direction === 'asc';
        let valueA: string;
        let valueB: string;

        switch (sort.active) {
          case 'name':
            valueA = (a.fullName || 'N/A').trim();
            valueB = (b.fullName || 'N/A').trim();
            break;
          case 'email':
            valueA = (a.email || '').trim();
            valueB = (b.email || '').trim();
            break;
          case 'role':
            valueA = (a.role || '').trim();
            valueB = (b.role || '').trim();
            break;
          default:
            return 0;
        }

        const comparison = valueA.localeCompare(valueB, undefined, { 
          sensitivity: 'case',
          numeric: true
        });
        
        return (comparison < 0 ? -1 : comparison > 0 ? 1 : 0) * (isAsc ? 1 : -1);
      });
    };
  }

  load() {
    this.api.getUsers().subscribe({
      next: (u) => {
        const usersWithStatus = u.map(user => ({
          ...user,
          isActive: user.isActive !== undefined ? user.isActive : true
        }));
        this.users.set(usersWithStatus);
        this.dataSource.data = usersWithStatus;
        
        setTimeout(() => {
          if (this.sort && !this.sort.active) {
            this.sort.sort({ id: 'name', start: 'asc', disableClear: false });
          }
        }, 0);
      },
      error: () => this.notification.error('Failed to load users.')
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  openChangeRoleDialog(user: UserManagement): void {
    if (!this.canChangeRole(user)) {
      if (user.id === this.myUserId) {
        this.notification.error('You cannot change your own role.');
      } else if (this.isAdminUser(user)) {
        this.notification.error('Admin role is immutable and cannot be changed.');
      }
      return;
    }

    const dialogData: ChangeRoleDialogData = {
      userId: user.id,
      userName: user.fullName || user.email,
      userEmail: user.email,
      currentRole: user.role
    };

    const dialogRef = this.dialog.open(ChangeRoleDialogComponent, {
      width: '500px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((newRole: string | undefined) => {
      if (newRole && newRole !== user.role) {
        const confirmData: ConfirmRoleChangeDialogData = {
          userName: user.fullName || user.email,
          userEmail: user.email,
          currentRole: user.role,
          newRole: newRole
        };

        const confirmDialogRef = this.dialog.open(ConfirmRoleChangeDialogComponent, {
          width: '500px',
          data: confirmData,
          disableClose: true
        });

        confirmDialogRef.afterClosed().subscribe((confirmed: boolean) => {
          if (confirmed) {
            this.updateRole(user.id, newRole);
          }
        });
      }
    });
  }

  updateRole(userId: string, newRole: string): void {
    this.api.updateRole({ userId, newRole }).subscribe({
      next: () => {
        this.notification.success('User role updated successfully');
        this.load();
      },
      error: (err) => {
        const message = err?.error?.message || 'Failed to update user role.';
        this.notification.error(message);
      }
    });
  }

  createUser() {
    if (this.createUserForm.invalid) {
      this.createUserForm.markAllAsTouched();
      return;
    }

    const dto = this.createUserForm.getRawValue();
    this.api.createUser({
      fullName: dto.fullName,
      email: dto.email,
      password: dto.password,
      phoneNumber: dto.phoneNumber,
      role: dto.role,
    }).subscribe({
      next: (res) => {
        const userId = res.data?.userId || 'N/A';
        this.notification.success(`User created successfully. User ID: ${userId}`);
        this.createUserForm.reset();
        this.createUserForm.patchValue({ role: 'Customer' });
        this.showCreateForm.set(false);
        this.load();
      },
      error: (err) => {
        const message = err?.error?.message || 'Failed to create user.';
        this.notification.error(message);
      }
    });
  }

  toggleUserActive(user: UserManagement) {
    const isCurrentlyActive = user.isActive !== false;
    const dialogData: ConfirmDeactivateUserDialogData = {
      userName: user.fullName || user.email,
      userEmail: user.email,
      isActive: isCurrentlyActive
    };

    const dialogRef = this.dialog.open(ConfirmDeactivateUserDialogComponent, {
      width: '500px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        const userId = user.id;
        
        this.api.toggleUserActive(userId).subscribe({
          next: () => {
            this.load();
            const action = isCurrentlyActive ? 'deactivated' : 'activated';
            this.notification.success(`User ${action} successfully`);
          },
          error: (err) => {
            const message = err?.error?.message || `Failed to ${isCurrentlyActive ? 'deactivate' : 'activate'} user.`;
            this.notification.error(message);
          }
        });
      }
    });
  }

  canToggleUserActive(user: UserManagement): boolean {
    return user.id !== this.myUserId && user.role?.toLowerCase() !== 'admin';
  }

  isUserActive(user: UserManagement): boolean {
    return user.isActive !== false;
  }
}