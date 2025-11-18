namespace EmployeeLeaveManagement.Models;

public class EmployeeLeaveAllocation
{
    public int EmployeeLeaveAllocationId { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }

    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;

    public int RemainingDays => TotalDays - UsedDays;
}

