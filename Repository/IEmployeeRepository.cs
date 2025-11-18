using EmployeeLeaveManagement.Models;

namespace EmployeeLeaveManagement.Repository;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByEmpCodeAsync(string empCode);
    Task<Employee?> GetByEmailAsync(string email);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(string department);
    Task<bool> EmpCodeExistsAsync(string empCode);
    Task<bool> EmailExistsAsync(string email);
}

