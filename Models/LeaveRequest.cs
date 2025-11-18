namespace EmployeeLeaveManagement.Models;

public class LeaveRequest
{
    public int LeaveRequestId { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // e.g., "Pending", "Approved", "Rejected"

    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}

