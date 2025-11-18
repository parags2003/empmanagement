using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Repository;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByEmpCodeAsync(string empCode)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.EmpCode == empCode);
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
    {
        return await _dbSet.Where(e => e.Department == department).ToListAsync();
    }

    public async Task<bool> EmpCodeExistsAsync(string empCode)
    {
        return await _dbSet.AnyAsync(e => e.EmpCode == empCode);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(e => e.Email == email);
    }

    public override async Task<Employee?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(e => e.LeaveRequests)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);
    }

    public override async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _dbSet
            .Include(e => e.LeaveRequests)
            .ToListAsync();
    }
}

