namespace EmployeeLeaveManagement.DTOs;

public class EmployeeDto
{
    public int EmployeeId { get; set; }
    public string EmpCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
}

