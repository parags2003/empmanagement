# Employee Leave Management

ASP.NET Core 8 MVC web application for managing employee leave requests.

## Project Structure

```
EmployeeLeaveManagement/
├── Controllers/          # MVC Controllers
├── Data/                # Entity Framework DbContext
├── DTOs/                # Data Transfer Objects
├── Models/              # Entity Models
├── Repository/          # Repository Pattern Implementation
├── Services/            # Business Logic Services
├── ViewModels/          # View Models for Views
├── Views/               # Razor Views
└── wwwroot/             # Static files (CSS, JS, images)
```

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or SQL Server Express)

## Setup

1. Update the connection string in `appsettings.json` to match your SQL Server instance.

2. Run migrations to create the database:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

3. Run the application:
```bash
dotnet run
```

## Features

- Clean architecture with Repository Pattern
- Dependency Injection configured
- Entity Framework Core with SQL Server
- MVC structure with proper separation of concerns

