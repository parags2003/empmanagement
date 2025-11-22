using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Repository;
using EmployeeLeaveManagement.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework Core with PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Try to get connection string from multiple sources
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// Log for debugging (without exposing sensitive data)
if (string.IsNullOrEmpty(connectionString))
{
    var envVars = Environment.GetEnvironmentVariables();
    var hasConnectionString = envVars.Keys.Cast<string>().Any(k => 
        k.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase));
    
    Console.WriteLine($"❌ Connection string not found!");
    Console.WriteLine($"   Environment variable 'ConnectionStrings__DefaultConnection' exists: {hasConnectionString}");
    Console.WriteLine($"   Available env vars with 'Connection': {string.Join(", ", envVars.Keys.Cast<string>().Where(k => k.Contains("Connection", StringComparison.OrdinalIgnoreCase)))}");
    
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. " +
        "Please set the environment variable 'ConnectionStrings__DefaultConnection' in Render.");
}
else
{
    // Log that connection string was found (but don't log the actual value for security)
    var connectionStringPreview = connectionString.Length > 50 
        ? connectionString.Substring(0, 50) + "..." 
        : connectionString;
    Console.WriteLine($"✅ Connection string found: {connectionStringPreview}");
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

// Test database connection and apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Check if database can connect
        var canConnect = dbContext.Database.CanConnect();
        if (canConnect)
        {
            app.Logger.LogInformation("✅ Successfully connected to PostgreSQL database.");
            
            // Apply pending migrations automatically (useful for Render deployments)
            try
            {
                app.Logger.LogInformation("🔄 Applying database migrations...");
                dbContext.Database.Migrate();
                app.Logger.LogInformation("✅ Database migrations applied successfully.");
            }
            catch (Exception migrateEx)
            {
                app.Logger.LogWarning(migrateEx, "⚠️ Could not apply migrations. Database may already be up to date.");
            }
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

