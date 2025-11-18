using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Repository;

public class LeaveAllocationRepository : Repository<EmployeeLeaveAllocation>, ILeaveAllocationRepository
{
    public LeaveAllocationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<EmployeeLeaveAllocation?> GetAllocationAsync(int employeeId, int leaveTypeId)
    {
        return await _dbSet.Include(a => a.LeaveType)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.LeaveTypeId == leaveTypeId);
    }

    public async Task<IEnumerable<EmployeeLeaveAllocation>> GetByEmployeeAsync(int employeeId)
    {
        return await _dbSet.Include(a => a.LeaveType)
            .Where(a => a.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task UpdateUsedDaysAsync(EmployeeLeaveAllocation allocation, int usedDays)
    {
        allocation.UsedDays = usedDays;
        _dbSet.Update(allocation);
        await _context.SaveChangesAsync();
    }
}

