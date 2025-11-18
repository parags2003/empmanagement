using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Repository;

public class LeaveRepository : Repository<LeaveRequest>, ILeaveRepository
{
    public LeaveRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId)
    {
        return await IncludeRelations()
            .Where(lr => lr.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByLeaveTypeIdAsync(int leaveTypeId)
    {
        return await IncludeRelations()
            .Where(lr => lr.LeaveTypeId == leaveTypeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status)
    {
        return await IncludeRelations()
            .Where(lr => lr.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAndStatusAsync(int employeeId, string status)
    {
        return await IncludeRelations()
            .Where(lr => lr.EmployeeId == employeeId && lr.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await IncludeRelations()
            .Where(lr => lr.StartDate >= startDate && lr.EndDate <= endDate)
            .ToListAsync();
    }

    private IQueryable<LeaveRequest> IncludeRelations()
    {
        return _dbSet
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.LeaveAllocations)
            .Include(lr => lr.LeaveType);
    }

    public override async Task<LeaveRequest?> GetByIdAsync(int id)
    {
        return await IncludeRelations()
            .FirstOrDefaultAsync(lr => lr.LeaveRequestId == id);
    }

    public override async Task<IEnumerable<LeaveRequest>> GetAllAsync()
    {
        return await IncludeRelations().ToListAsync();
    }
}

