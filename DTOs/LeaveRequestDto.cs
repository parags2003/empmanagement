namespace EmployeeLeaveManagement.DTOs;

public class LeaveRequestDto
{
    public int LeaveRequestId { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // Additional properties for display
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public string EmpCode { get; set; } = string.Empty;
    public int RequestedDays { get; set; }
    public int RemainingDays { get; set; }
}

