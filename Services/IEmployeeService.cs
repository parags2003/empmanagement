using EmployeeLeaveManagement.DTOs;
using EmployeeLeaveManagement.Models;

namespace EmployeeLeaveManagement.Services;

public interface IEmployeeService
{
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByEmpCodeAsync(string empCode);
    Task<EmployeeDto?> GetEmployeeByEmailAsync(string email);
    Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string department);
    Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto);
    Task<EmployeeDto> UpdateEmployeeAsync(int id, EmployeeDto employeeDto);
    Task<bool> DeleteEmployeeAsync(int id);
    Task<bool> EmpCodeExistsAsync(string empCode);
    Task<bool> EmailExistsAsync(string email);
}
