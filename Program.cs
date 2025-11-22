using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Repository;
using EmployeeLeaveManagement.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework Core with PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Helper function to convert PostgreSQL URL format to standard connection string
string ConvertPostgresUrlToConnectionString(string connString)
{
    // If it's already in standard format (contains "Host="), return as is
    if (connString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
    {
        // Ensure SSL parameters are present for cloud databases (like Render)
        if (!connString.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase) &&
            !connString.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        {
            // Add SSL parameters for cloud PostgreSQL (Render, etc.)
            if (connString.EndsWith(";"))
            {
                connString += "SSL Mode=Require;Trust Server Certificate=true";
            }
            else
            {
                connString += ";SSL Mode=Require;Trust Server Certificate=true";
            }
        }
        return connString;
    }

    // If it's in URL format (postgresql://...), convert it
    if (connString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
        connString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(connString);
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            var username = uri.UserInfo.Split(':')[0];
            var password = uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : "";

            var converted = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
            return converted;
        }
        catch
        {
            // If parsing fails, return original string
            return connString;
        }
    }

    return connString;
}

// Convert PostgreSQL URL format to standard connection string format if needed
connectionString = ConvertPostgresUrlToConnectionString(connectionString);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }));

// Register Repositories for Dependency Injection
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ILeaveAllocationRepository, LeaveAllocationRepository>();

// Register Services for Dependency Injection
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

var app = builder.Build();

// Test database connection on startup (for both development and production)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        var canConnect = dbContext.Database.CanConnect();
        if (canConnect)
        {
            app.Logger.LogInformation("✅ Successfully connected to PostgreSQL database.");
        }
        else
        {
            app.Logger.LogWarning("⚠️ Cannot connect to PostgreSQL database.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Failed to connect to PostgreSQL database. Please check your connection string.");
        // In production, log the error but don't crash - let the app start and show errors on pages
        if (app.Environment.IsDevelopment())
        {
            throw; // In development, throw to see the error immediately
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

