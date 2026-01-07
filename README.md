# Vehicle Service Management System

A comprehensive full-stack web application designed to streamline vehicle service center operations. The system automates service booking, technician assignment, service tracking, inventory management, billing, and provides detailed reports and analytics.

**Repository**: [https://github.com/abahadur29/vehicle-service-management-system](https://github.com/abahadur29/vehicle-service-management-system)

## Overview

This system helps vehicle service centers manage customer vehicles, service requests, technician workloads, and inventory efficiently. It eliminates manual tracking errors, speeds up service processing, and provides better visibility into service history, revenue, and operational metrics.

## Key Features

- **User & Security Management**: JWT-based authentication with role-based access control (RBAC)
- **Vehicle Management**: Register and manage customer vehicles with service history
- **Service Request Management**: Book services, track status, and manage service lifecycle
- **Technician Assignment**: Assign technicians to service requests and track workload
- **Inventory Management**: Manage parts inventory with stock tracking and pricing
- **Billing & Payments**: Generate invoices, track payments, and manage outstanding balances
- **Reports & Dashboards**: Revenue reports, technician performance, service analytics, and monthly statistics
- **Notifications**: Real-time notifications for service status updates and assignments

## Technology Stack

### Backend
- **.NET 8.0** - ASP.NET Core Web API
- **Entity Framework Core 8.0** - ORM for database operations
- **SQL Server** - Database
- **JWT Bearer Authentication** - Secure API authentication
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Request validation
- **AutoMapper** - Object mapping
- **Swagger/OpenAPI** - API documentation

### Frontend
- **Angular 21.0** - Frontend framework
- **TypeScript** - Programming language
- **Angular Material** - UI component library
- **RxJS** - Reactive programming
- **Angular Signals** - State management

### Testing
- **xUnit** - Unit testing framework
- **Integration Tests** - API endpoint testing
- **Postman** - API testing collection

## User Roles

The system supports four distinct user roles:

### Admin

- Manage users, roles, and permissions
- Manage service categories and pricing
- Manage inventory and parts
- Full system access and configuration

### Service Manager

- Assign service tasks to technicians
- Monitor technician workload
- Approve and manage service requests
- Manage inventory stock levels
- **Dashboards & Reports**:
  - View pending vs completed services
  - Monitor technician workload and performance
  - View monthly revenue reports
  - View service history per vehicle

### Technician

- View assigned service tasks
- Update service status and progress
- Complete services and record parts used
- View service history
- Cannot view pricing information

### Customer

- Register and manage vehicles
- Book service requests
- View service history and status
- View invoices and make payments
- Track service progress

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js (v18 or higher)** - [Download here](https://nodejs.org/)
- **npm** (comes with Node.js)
- **SQL Server** (Express edition is fine) - [Download here](https://www.microsoft.com/sql-server/sql-server-downloads)
- **Visual Studio 2022** or **Visual Studio Code** (recommended)
- **Angular CLI** (will be installed globally during setup)

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/abahadur29/vehicle-service-management-system.git
cd vehicle-service-management-system
```

### 2. Database Setup

1. Install SQL Server (if not already installed)

2. **Update Connection String**: Open `backend/VehicleServiceManagement/VehicleServiceManagement.API/appsettings.json`

   Update the `DefaultConnection` string with your SQL Server instance:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=VehicleServiceDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

   For SQL Server Express, use:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER_NAME\\SQLEXPRESS;Database=VehicleServiceDB;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

   Replace `YOUR_SERVER_NAME` with your SQL Server instance name (e.g., `localhost` or your computer name).

### 3. Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend/VehicleServiceManagement/VehicleServiceManagement.API
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

   If you encounter issues with Entity Framework tools, install them first:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. (Optional) Verify the database was created in SQL Server Management Studio

### 4. Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend/vehicle-service-management-ui
   ```

2. Install Angular CLI globally (if not already installed):
   ```bash
   npm install -g @angular/cli
   ```

3. Install project dependencies:
   ```bash
   npm install
   ```

## How to Run

### Running the Backend

1. Navigate to the backend project directory:
   ```bash
   cd backend/VehicleServiceManagement/VehicleServiceManagement.API
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

   Or use Visual Studio:
   - Open the solution file `backend/VehicleServiceManagement/VehicleServiceManagement.sln`
   - Press F5 or click the "Run" button

3. The API will start on:
   - **HTTP**: `http://localhost:5293`
   - **HTTPS**: `https://localhost:7033`

4. **Access Swagger UI**:
   - Open your browser and navigate to: `https://localhost:7033/swagger`
   - This provides interactive API documentation

### Running the Frontend

1. Navigate to the frontend directory:
   ```bash
   cd frontend/vehicle-service-management-ui
   ```

2. Start the Angular development server:
   ```bash
   ng serve
   ```

   Or:
   ```bash
   npm start
   ```

3. **Access the application**:
   - Open your browser and navigate to: `http://localhost:4200`
   - The frontend is configured to communicate with the backend API running on `https://localhost:7033`

### Running Both Together

**Option 1: Separate Terminals**

- Open two terminal windows/command prompts
- Run backend in one terminal
- Run frontend in another terminal

**Option 2: PowerShell (Windows)**

```powershell
# Terminal 1
cd "backend\VehicleServiceManagement\VehicleServiceManagement.API"
dotnet run

# Terminal 2
cd "frontend\vehicle-service-management-ui"
ng serve
```

## Project Structure

```
vehicle-service-management-system/
├── backend/
│   └── VehicleServiceManagement/
│       ├── VehicleServiceManagement.API/
│       │   ├── Application/
│       │   │   ├── DTOs/              # Data Transfer Objects
│       │   │   ├── Features/           # CQRS handlers (Commands & Queries)
│       │   │   ├── Mappings/           # AutoMapper profiles
│       │   │   └── Validators/         # FluentValidation validators
│       │   ├── Controllers/            # API Controllers
│       │   ├── Core/
│       │   │   ├── Entities/           # Domain entities
│       │   │   └── Interfaces/         # Repository interfaces
│       │   ├── Infrastructure/
│       │   │   ├── Data/               # DbContext and seed data
│       │   │   └── Services/           # Infrastructure services
│       │   ├── Middlewares/            # Global exception handling
│       │   ├── Migrations/             # EF Core database migrations
│       │   ├── Program.cs              # Application entry point
│       │   └── appsettings.json        # Configuration
│       ├── VehicleServiceManagement.Tests/     # Unit tests
│       ├── VehicleServiceManagement.IntegrationTests/  # Integration tests
│       └── VehicleServiceManagement.sln        # Solution file
│
├── frontend/
│   └── vehicle-service-management-ui/
│       ├── src/
│       │   └── app/
│       │       ├── core/               # Core modules (auth, guards, services)
│       │       ├── features/           # Feature modules
│       │       ├── shared/             # Shared components, models, pipes
│       │       └── environments/       # Environment configuration
│       ├── package.json                # Node dependencies
│       └── angular.json                # Angular configuration
│
├── Deliverables/
│   ├── Database schema.docx
│   ├── UI Screenshots.pdf
│   ├── LINQ_USAGE_OVERVIEW.pdf
│   ├── SETUP_GUIDE_DOTNET8_ANGULAR20.pdf
│   └── FRONTEND_(This setup is for Angular 21).pdf
│
├── README.md                           # This file
└── SETUP_INSTRUCTIONS.md              # Detailed setup guide
```

## Configuration

### Backend Configuration

Edit `backend/VehicleServiceManagement/VehicleServiceManagement.API/appsettings.json`:

- **ConnectionStrings**: Update SQL Server connection string
- **Jwt**: Configure JWT settings (Key, Issuer, Audience)
- **Logging**: Adjust log levels as needed

### Frontend Configuration

The frontend communicates with the backend API. The API base URL is configured in `frontend/vehicle-service-management-ui/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7033',
};
```

Ensure this matches your backend HTTPS port.

## Testing

### Backend Tests

Run unit and integration tests:

```bash
cd backend/VehicleServiceManagement/VehicleServiceManagement.Tests
dotnet test
```

For integration tests:

```bash
cd backend/VehicleServiceManagement/VehicleServiceManagement.IntegrationTests
dotnet test
```

### Frontend Tests

Run Angular tests:

```bash
cd frontend/vehicle-service-management-ui
ng test
```

## API Documentation

Once the backend is running, access the Swagger UI at:

**https://localhost:7033/swagger**

This interactive documentation allows you to:

- View all available endpoints
- Test API calls directly
- See request/response schemas
- Authenticate and test protected endpoints

## Default Test Users (Development)

The system includes seed data for development. After running the application and migrations, the following default users are automatically created:

| Role | Email | Password | Full Name | Permissions |
|------|-------|----------|-----------|-------------|
| Admin | admin@vehicle.com | Admin@123 | System Administrator | Full system access - Manage users, categories, and inventory |
| Service Manager | manager@vehicle.com | Manager@123 | Ramesh Kumar | Assign service tasks, monitor technician workload, approve services, and view reports & dashboards |
| Technician | tech@vehicle.com | Tech@123 | Rajiv Pratap | View assigned tasks, update service status, complete services |
| Customer | customer@vehicle.com | Customer@123 | Rahul Verma | Book services, view service history, manage vehicles, and make payments |

**Additional Test Users:**

- **Service Managers**: `sunita.manager@vehicle.com`, `amit.manager@vehicle.com` (Password: `Manager@123`)
- **Technicians**: `priya.tech@vehicle.com`, `anita.tech@vehicle.com`, `vikram.tech@vehicle.com`, `kavita.tech@vehicle.com`, `arjun.tech@vehicle.com` (Password: `Tech@123`)
- **Customers**: `sneha.customer@vehicle.com`, `mohit.customer@vehicle.com`, and 8 more (Password: `Customer@123`)

**Note**: All default users are automatically seeded when the application runs in Development mode. The primary customer user (`customer@vehicle.com`) has sample vehicles and service history for testing purposes.

## Troubleshooting

### Database Connection Issues

- Verify SQL Server is running
- Check the connection string in `backend/VehicleServiceManagement/VehicleServiceManagement.API/appsettings.json`
- Ensure SQL Server allows Windows Authentication or update connection string with SQL credentials
- Try using `localhost` instead of the computer name
- For LocalDB, ensure it's installed and running

### Port Already in Use

**Backend**: Change port in `backend/VehicleServiceManagement/VehicleServiceManagement.API/Properties/launchSettings.json`

**Frontend**: Use `ng serve --port <port-number>`

### Migration Issues

- Delete existing migrations and recreate: `dotnet ef migrations add InitialCreate`
- Ensure database exists: `dotnet ef database update`
- Check that the connection string is correct

### Frontend Dependencies Issues

- Delete `node_modules` folder and `package-lock.json`
- Run `npm install` again
- Clear npm cache: `npm cache clean --force`

### CORS Issues

- Ensure backend CORS is configured in `Program.cs`
- Verify frontend API base URL matches backend URL
- Check that both applications are running

### SSL Certificate Issues

- If you encounter SSL certificate errors, you may need to trust the development certificate:
  ```bash
  dotnet dev-certs https --trust
  ```

## Additional Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Angular Documentation](https://angular.dev/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [JWT Authentication](https://jwt.io/)

## License

This project is part of a capstone project.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Repository

GitHub: [https://github.com/abahadur29/vehicle-service-management-system](https://github.com/abahadur29/vehicle-service-management-system)

