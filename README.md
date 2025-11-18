# Employee Leave Management

ASP.NET Core 8 MVC application for managing employees, leave types, and leave requests.  
The project now uses Entity Framework Core against PostgreSQL so it can run on free tiers such as Render + Render PostgreSQL.

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

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- PostgreSQL 14+ (local or hosted). For local dev you can use Docker or [Postgres.app](https://postgresapp.com/) / [Windows installer](https://www.postgresql.org/download/).

## Local Setup

1. **Configure the DB connection**  
   Update `appsettings.json` → `ConnectionStrings:DefaultConnection` with your PostgreSQL connection string, e.g.  
   ```
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=EmpLeaveDb;Username=postgres;Password=yourpassword"
   }
   ```
2. **Apply EF Core migrations** (creates the schema, tables, seed data):  
   ```bash
   dotnet ef database update
   ```
3. **Run the app**  
   ```bash
   dotnet run
   ```

## Deploying to Render (free tier)

1. **Push to GitHub.**
2. **Create a free Render PostgreSQL instance** and copy its connection string.
3. **Create a Render Web Service** (runtime: .NET 8). Use build command `dotnet restore && dotnet publish -c Release -o out` and start command `dotnet out/EmployeeLeaveManagement.dll`.
4. In Render → **Environment** add:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__DefaultConnection=<your Render PostgreSQL connection string>`
5. Deploy. Every git push to the selected branch will trigger an automatic build/release.

## Features

- Clean architecture with Repository Pattern
- PostgreSQL + Entity Framework Core 8
- MVC structure with proper separation of concerns

