using EmployeeLeaveManagement.Services;
using EmployeeLeaveManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveService _leaveService;

    public HomeController(
        ILogger<HomeController> logger,
        IEmployeeService employeeService,
        ILeaveService leaveService)
    {
        _logger = logger;
        _employeeService = employeeService;
        _leaveService = leaveService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            var approvedLeaves = await _leaveService.GetLeaveRequestsByStatusAsync("Approved");
            
            // Get current date range for active leaves
            var today = DateTime.Today;
            var activeLeaves = approvedLeaves.Where(l => l.StartDate <= today && l.EndDate >= today);

            var viewModel = new DashboardViewModel
            {
                TotalEmployees = employees.Count(),
                ActiveEmployees = employees.Count(), // All employees are considered active for now
                OnLeave = activeLeaves.Count(),
                NewHires = 0, // Can be calculated based on creation date if needed
                RecentEmployees = employees
                    .OrderByDescending(e => e.EmployeeId)
                    .Take(5)
                    .Select(e => new RecentEmployeeViewModel
                    {
                        Name = e.Name,
                        Position = e.Position,
                        Department = e.Department,
                        Status = "Active"
                    })
                    .ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            return View(new DashboardViewModel());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }
}

