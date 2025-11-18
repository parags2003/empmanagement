using EmployeeLeaveManagement.Models;

namespace EmployeeLeaveManagement.Repository;

public interface ILeaveAllocationRepository : IRepository<EmployeeLeaveAllocation>
{
    Task<EmployeeLeaveAllocation?> GetAllocationAsync(int employeeId, int leaveTypeId);
    Task<IEnumerable<EmployeeLeaveAllocation>> GetByEmployeeAsync(int employeeId);
    Task UpdateUsedDaysAsync(EmployeeLeaveAllocation allocation, int usedDays);
}

