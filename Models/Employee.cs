namespace EmployeeLeaveManagement.Models;

public class Employee
{
    public int EmployeeId { get; set; }
    public string EmpCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }

    // Navigation properties
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<EmployeeLeaveAllocation> LeaveAllocations { get; set; } = new List<EmployeeLeaveAllocation>();
}

