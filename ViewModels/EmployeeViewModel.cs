using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EmployeeLeaveManagement.ViewModels;

public class EmployeeViewModel
{
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Employee Code is required")]
    [Display(Name = "Employee Code")]
    public string EmpCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required")]
    [Display(Name = "Department")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position is required")]
    [Display(Name = "Position")]
    public string Position { get; set; } = string.Empty;

    [Display(Name = "Photo Path")]
    public string? PhotoPath { get; set; }

    [Display(Name = "Photo Upload")]
    public IFormFile? PhotoUpload { get; set; }
}

