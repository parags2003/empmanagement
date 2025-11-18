namespace EmployeeLeaveManagement.ViewModels;

public class LeaveRequestListViewModel
{
    public int LeaveRequestId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmpCode { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public int RemainingDays { get; set; }
}

