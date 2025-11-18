using EmployeeLeaveManagement.DTOs;

namespace EmployeeLeaveManagement.Services;

public interface ILeaveService
{
    Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id);
    Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync();
    Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByLeaveTypeIdAsync(int leaveTypeId);
    Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByStatusAsync(string status);
    Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAndStatusAsync(int employeeId, string status);
    Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<LeaveRequestDto> CreateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<LeaveRequestDto> UpdateLeaveRequestAsync(int id, LeaveRequestDto leaveRequestDto);
    Task<bool> DeleteLeaveRequestAsync(int id);
    Task<LeaveRequestDto> UpdateLeaveRequestStatusAsync(int id, string status);
}

