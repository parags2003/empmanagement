using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Add DbSet properties for your entities here
    public DbSet<Employee> Employees { get; set; }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<EmployeeLeaveAllocation> EmployeeLeaveAllocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Leave Types
        modelBuilder.Entity<LeaveType>().HasData(
            new LeaveType { LeaveTypeId = 1, LeaveName = "Casual Leave", DefaultDays = 10 },
            new LeaveType { LeaveTypeId = 2, LeaveName = "Annual Leave", DefaultDays = 10 },
            new LeaveType { LeaveTypeId = 3, LeaveName = "Medical Leave", DefaultDays = 5 }
        );

        modelBuilder.Entity<EmployeeLeaveAllocation>()
            .HasIndex(x => new { x.EmployeeId, x.LeaveTypeId })
            .IsUnique();
        modelBuilder.Entity<LeaveRequest>()
            .HasOne(lr => lr.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(lr => lr.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LeaveRequest>()
            .HasOne(lr => lr.LeaveType)
            .WithMany(lt => lt.LeaveRequests)
            .HasForeignKey(lr => lr.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

