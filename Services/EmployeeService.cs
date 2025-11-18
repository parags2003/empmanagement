using EmployeeLeaveManagement.DTOs;
using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.Repository;

namespace EmployeeLeaveManagement.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IRepository<LeaveType> _leaveTypeRepository;
    private readonly ILeaveAllocationRepository _leaveAllocationRepository;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IRepository<LeaveType> leaveTypeRepository,
        ILeaveAllocationRepository leaveAllocationRepository)
    {
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _leaveAllocationRepository = leaveAllocationRepository;
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetEmployeeByEmpCodeAsync(string empCode)
    {
        var employee = await _employeeRepository.GetByEmpCodeAsync(empCode);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto?> GetEmployeeByEmailAsync(string email)
    {
        var employee = await _employeeRepository.GetByEmailAsync(email);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string department)
    {
        var employees = await _employeeRepository.GetByDepartmentAsync(department);
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto)
    {
        var employee = MapToEntity(employeeDto);
        var createdEmployee = await _employeeRepository.AddAsync(employee);
        await AssignDefaultLeaveAllocationsAsync(createdEmployee.EmployeeId);
        return MapToDto(createdEmployee);
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(int id, EmployeeDto employeeDto)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
            throw new KeyNotFoundException($"Employee with ID {id} not found.");

        employee.EmpCode = employeeDto.EmpCode;
        employee.Name = employeeDto.Name;
        employee.Email = employeeDto.Email;
        employee.Department = employeeDto.Department;
        employee.Position = employeeDto.Position;
        employee.PhotoPath = employeeDto.PhotoPath;

        await _employeeRepository.UpdateAsync(employee);
        return MapToDto(employee);
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
            return false;

        await _employeeRepository.DeleteAsync(employee);
        return true;
    }

    public async Task<bool> EmpCodeExistsAsync(string empCode)
    {
        return await _employeeRepository.EmpCodeExistsAsync(empCode);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _employeeRepository.EmailExistsAsync(email);
    }

    private async Task AssignDefaultLeaveAllocationsAsync(int employeeId)
    {
        var leaveTypes = await _leaveTypeRepository.GetAllAsync();
        foreach (var leaveType in leaveTypes)
        {
            var allocation = new EmployeeLeaveAllocation
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveType.LeaveTypeId,
                TotalDays = leaveType.DefaultDays,
                UsedDays = 0
            };

            await _leaveAllocationRepository.AddAsync(allocation);
        }
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            EmpCode = employee.EmpCode,
            Name = employee.Name,
            Email = employee.Email,
            Department = employee.Department,
            Position = employee.Position,
            PhotoPath = employee.PhotoPath
        };
    }

    private static Employee MapToEntity(EmployeeDto employeeDto)
    {
        return new Employee
        {
            EmployeeId = employeeDto.EmployeeId,
            EmpCode = employeeDto.EmpCode,
            Name = employeeDto.Name,
            Email = employeeDto.Email,
            Department = employeeDto.Department,
            Position = employeeDto.Position,
            PhotoPath = employeeDto.PhotoPath
        };
    }
}
