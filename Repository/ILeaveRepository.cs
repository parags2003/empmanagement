using EmployeeLeaveManagement.Models;

namespace EmployeeLeaveManagement.Repository;

public interface ILeaveRepository : IRepository<LeaveRequest>
{
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveRequest>> GetByLeaveTypeIdAsync(int leaveTypeId);
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAndStatusAsync(int employeeId, string status);
    Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}

