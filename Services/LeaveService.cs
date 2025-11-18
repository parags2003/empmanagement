using EmployeeLeaveManagement.DTOs;
using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.Repository;

namespace EmployeeLeaveManagement.Services;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IRepository<LeaveType> _leaveTypeRepository;
    private readonly ILeaveAllocationRepository _leaveAllocationRepository;

    public LeaveService(
        ILeaveRepository leaveRepository,
        IEmployeeRepository employeeRepository,
        IRepository<LeaveType> leaveTypeRepository,
        ILeaveAllocationRepository leaveAllocationRepository)
    {
        _leaveRepository = leaveRepository;
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _leaveAllocationRepository = leaveAllocationRepository;
    }

    public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(id);
        return leaveRequest == null ? null : MapToDto(leaveRequest);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync()
    {
        var leaveRequests = await _leaveRepository.GetAllAsync();
        return leaveRequests.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
    {
        var leaveRequests = await _leaveRepository.GetByEmployeeIdAsync(employeeId);
        return leaveRequests.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByLeaveTypeIdAsync(int leaveTypeId)
    {
        var leaveRequests = await _leaveRepository.GetByLeaveTypeIdAsync(leaveTypeId);
        return leaveRequests.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByStatusAsync(string status)
    {
        var leaveRequests = await _leaveRepository.GetByStatusAsync(status);
        return leaveRequests.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAndStatusAsync(int employeeId, string status)
    {
        var leaveRequests = await _leaveRepository.GetByEmployeeAndStatusAsync(employeeId, status);
        return leaveRequests.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var leaveRequests = await _leaveRepository.GetByDateRangeAsync(startDate, endDate);
        return leaveRequests.Select(MapToDto);
    }

    public async Task<LeaveRequestDto> CreateLeaveRequestAsync(LeaveRequestDto leaveRequestDto)
    {
        // Validate that Employee exists
        var employee = await _employeeRepository.GetByIdAsync(leaveRequestDto.EmployeeId);
        if (employee == null)
            throw new KeyNotFoundException($"Employee with ID {leaveRequestDto.EmployeeId} not found.");

        // Validate that LeaveType exists
        var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveRequestDto.LeaveTypeId);
        if (leaveType == null)
            throw new KeyNotFoundException($"LeaveType with ID {leaveRequestDto.LeaveTypeId} not found.");

        var allocation = await _leaveAllocationRepository.GetAllocationAsync(leaveRequestDto.EmployeeId, leaveRequestDto.LeaveTypeId);
        if (allocation == null)
            throw new InvalidOperationException("Leave allocation not found for employee.");

        var requestedDays = CalculateRequestedDays(leaveRequestDto.StartDate, leaveRequestDto.EndDate);
        if (requestedDays > allocation.RemainingDays)
            throw new InvalidOperationException($"Insufficient leave balance. Remaining days: {allocation.RemainingDays}");

        leaveRequestDto.Status = string.IsNullOrWhiteSpace(leaveRequestDto.Status) ? "Pending" : leaveRequestDto.Status;

        var leaveRequest = MapToEntity(leaveRequestDto);
        var createdLeaveRequest = await _leaveRepository.AddAsync(leaveRequest);
        return MapToDto(createdLeaveRequest);
    }

    public async Task<LeaveRequestDto> UpdateLeaveRequestAsync(int id, LeaveRequestDto leaveRequestDto)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(id);
        if (leaveRequest == null)
            throw new KeyNotFoundException($"LeaveRequest with ID {id} not found.");

        // Validate that Employee exists if changed
        if (leaveRequest.EmployeeId != leaveRequestDto.EmployeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(leaveRequestDto.EmployeeId);
            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID {leaveRequestDto.EmployeeId} not found.");
        }

        // Validate that LeaveType exists if changed
        if (leaveRequest.LeaveTypeId != leaveRequestDto.LeaveTypeId)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveRequestDto.LeaveTypeId);
            if (leaveType == null)
                throw new KeyNotFoundException($"LeaveType with ID {leaveRequestDto.LeaveTypeId} not found.");
        }

        leaveRequest.EmployeeId = leaveRequestDto.EmployeeId;
        leaveRequest.LeaveTypeId = leaveRequestDto.LeaveTypeId;
        leaveRequest.StartDate = leaveRequestDto.StartDate;
        leaveRequest.EndDate = leaveRequestDto.EndDate;
        leaveRequest.Reason = leaveRequestDto.Reason;
        leaveRequest.Status = leaveRequestDto.Status;

        await _leaveRepository.UpdateAsync(leaveRequest);
        return MapToDto(leaveRequest);
    }

    public async Task<bool> DeleteLeaveRequestAsync(int id)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(id);
        if (leaveRequest == null)
            return false;

        if (leaveRequest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            await AdjustAllocationAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId,
                -CalculateRequestedDays(leaveRequest.StartDate, leaveRequest.EndDate));
        }

        await _leaveRepository.DeleteAsync(leaveRequest);
        return true;
    }

    public async Task<LeaveRequestDto> UpdateLeaveRequestStatusAsync(int id, string status)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(id);
        if (leaveRequest == null)
            throw new KeyNotFoundException($"LeaveRequest with ID {id} not found.");

        var normalizedStatus = status ?? "Pending";
        var isCurrentlyApproved = leaveRequest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase);
        var willBeApproved = normalizedStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase);

        if (!isCurrentlyApproved && willBeApproved)
        {
            await EnsureAllocationAsync(leaveRequest, CalculateRequestedDays(leaveRequest.StartDate, leaveRequest.EndDate));
        }
        else if (isCurrentlyApproved && !willBeApproved)
        {
            await AdjustAllocationAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId,
                -CalculateRequestedDays(leaveRequest.StartDate, leaveRequest.EndDate));
        }

        leaveRequest.Status = normalizedStatus;
        await _leaveRepository.UpdateAsync(leaveRequest);
        return MapToDto(leaveRequest);
    }

    private static int CalculateRequestedDays(DateTime start, DateTime end)
    {
        return (end.Date - start.Date).Days + 1;
    }

    private async Task EnsureAllocationAsync(LeaveRequest leaveRequest, int requestedDays)
    {
        var allocation = await _leaveAllocationRepository.GetAllocationAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId);
        if (allocation == null)
            throw new InvalidOperationException("Leave allocation not found for employee.");

        if (requestedDays > allocation.RemainingDays)
            throw new InvalidOperationException($"Insufficient leave balance. Remaining days: {allocation.RemainingDays}");

        allocation.UsedDays += requestedDays;
        await _leaveAllocationRepository.UpdateAsync(allocation);
    }

    private async Task AdjustAllocationAsync(int employeeId, int leaveTypeId, int daysDelta)
    {
        var allocation = await _leaveAllocationRepository.GetAllocationAsync(employeeId, leaveTypeId);
        if (allocation == null)
            return;

        allocation.UsedDays = Math.Max(0, allocation.UsedDays + daysDelta);
        await _leaveAllocationRepository.UpdateAsync(allocation);
    }

    private LeaveRequestDto MapToDto(LeaveRequest leaveRequest)
    {
        var requestedDays = CalculateRequestedDays(leaveRequest.StartDate, leaveRequest.EndDate);
        var allocation = leaveRequest.Employee?.LeaveAllocations?.FirstOrDefault(a => a.LeaveTypeId == leaveRequest.LeaveTypeId);
        var remainingDays = allocation?.RemainingDays ?? 0;

        return new LeaveRequestDto
        {
            LeaveRequestId = leaveRequest.LeaveRequestId,
            EmployeeId = leaveRequest.EmployeeId,
            LeaveTypeId = leaveRequest.LeaveTypeId,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status,
            EmployeeName = leaveRequest.Employee?.Name ?? string.Empty,
            EmpCode = leaveRequest.Employee?.EmpCode ?? string.Empty,
            LeaveTypeName = leaveRequest.LeaveType?.LeaveName ?? string.Empty,
            RemainingDays = remainingDays,
            RequestedDays = requestedDays
        };
    }

    private static LeaveRequest MapToEntity(LeaveRequestDto leaveRequestDto)
    {
        return new LeaveRequest
        {
            LeaveRequestId = leaveRequestDto.LeaveRequestId,
            EmployeeId = leaveRequestDto.EmployeeId,
            LeaveTypeId = leaveRequestDto.LeaveTypeId,
            StartDate = leaveRequestDto.StartDate,
            EndDate = leaveRequestDto.EndDate,
            Reason = leaveRequestDto.Reason,
            Status = leaveRequestDto.Status
        };
    }
}

