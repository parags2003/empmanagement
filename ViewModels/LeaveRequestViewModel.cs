using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.ViewModels;

public class LeaveRequestViewModel
{
    public int LeaveRequestId { get; set; }

    [Required(ErrorMessage = "Employee is required")]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Leave Type is required")]
    [Display(Name = "Leave Type")]
    public int LeaveTypeId { get; set; }

    [Required(ErrorMessage = "Start Date is required")]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End Date is required")]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Reason is required")]
    [Display(Name = "Reason")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public string Status { get; set; } = "Pending";

    // Display properties
    [Display(Name = "Employee Name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Display(Name = "Leave Type")]
    public string LeaveTypeName { get; set; } = string.Empty;

    // Dropdown lists
    public List<EmployeeDropdownViewModel> Employees { get; set; } = new();
    public List<LeaveTypeDropdownViewModel> LeaveTypes { get; set; } = new();

    public int AvailableDays { get; set; }
}

public class EmployeeDropdownViewModel
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmpCode { get; set; } = string.Empty;
}

public class LeaveTypeDropdownViewModel
{
    public int LeaveTypeId { get; set; }
    public string LeaveName { get; set; } = string.Empty;
}

