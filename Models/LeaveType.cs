namespace EmployeeLeaveManagement.Models;

public class LeaveType
{
    public int LeaveTypeId { get; set; }
    public string LeaveName { get; set; } = string.Empty;
    public int DefaultDays { get; set; }

    // Navigation properties
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}

