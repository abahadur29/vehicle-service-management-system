# Vehicle Service Management System

This is a full-stack application for managing vehicle services, technicians, and billing.

## Tech Stack
- **Backend:** ASP.NET Core Web API (.NET 8/9), Entity Framework Core, SQL Server, JWT Authentication.
- **Frontend:** Angular 18+, TypeScript, Angular Material, Reactive Forms.

## Setup Instructions

### Backend
1. Navigate to `VehicleServiceManagement/VehicleServiceManagement.API`.
2. Update `appsettings.json` with your SQL Server connection string.
3. Run `dotnet ef database update` to apply migrations.
4. Run `dotnet run` or start from Visual Studio.

### Frontend
1. Navigate to `vehicle-service-management-ui`.
2. Run `npm install` to install dependencies.
3. Run `ng serve` to start the development server.
4. Open `http://localhost:4200` in your browser.

## User Roles
- **Admin:** Manage users, inventory, and view reports.
- **Manager:** Assign tasks and monitor workload.
- **Technician:** Update task status and complete services.
- **Customer:** Book services and track vehicle history.


```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
