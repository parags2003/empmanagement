using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.ViewModels;

public class LeaveAllocationViewModel
{
    [Display(Name = "Employee")]
    [Required(ErrorMessage = "Employee is required")]
    public int SelectedEmployeeId { get; set; }

    [Display(Name = "Leave Type")]
    [Required(ErrorMessage = "Leave type is required")]
    public int SelectedLeaveTypeId { get; set; }

    [Display(Name = "Total Days")]
    [Range(0, 365, ErrorMessage = "Total days must be between 0 and 365")]
    public int TotalDays { get; set; }

    public int CurrentBalance { get; set; }

    public List<EmployeeDropdownViewModel> Employees { get; set; } = new();
    public List<LeaveTypeDropdownViewModel> LeaveTypes { get; set; } = new();
    public List<LeaveAllocationItemViewModel> Allocations { get; set; } = new();
}

public class LeaveAllocationItemViewModel
{
    public string LeaveTypeName { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
}

