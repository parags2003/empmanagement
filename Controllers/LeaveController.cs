using EmployeeLeaveManagement.Repository;
using EmployeeLeaveManagement.Services;
using EmployeeLeaveManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeLeaveManagement.Controllers;

public class LeaveController : Controller
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;
    private readonly IRepository<Models.LeaveType> _leaveTypeRepository;
    private readonly ILeaveAllocationRepository _leaveAllocationRepository;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(
        ILeaveService leaveService,
        IEmployeeService employeeService,
        IRepository<Models.LeaveType> leaveTypeRepository,
        ILeaveAllocationRepository leaveAllocationRepository,
        ILogger<LeaveController> logger)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
        _leaveTypeRepository = leaveTypeRepository;
        _leaveAllocationRepository = leaveAllocationRepository;
        _logger = logger;
    }

    // GET: Leave/Apply
    public async Task<IActionResult> Apply()
    {
        try
        {
            var viewModel = new LeaveRequestViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Status = "Pending"
            };

            await PopulateDropdownsAsync(viewModel);

            if (viewModel.EmployeeId == 0 && viewModel.Employees.Any())
            {
                viewModel.EmployeeId = viewModel.Employees.First().EmployeeId;
            }

            if (viewModel.LeaveTypeId == 0 && viewModel.LeaveTypes.Any())
            {
                viewModel.LeaveTypeId = viewModel.LeaveTypes.First().LeaveTypeId;
            }

            await SetAvailableDaysAsync(viewModel);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading apply leave page");
            TempData["ErrorMessage"] = "An error occurred while loading the page.";
            return RedirectToAction(nameof(List));
        }
    }

    // POST: Leave/Apply
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(LeaveRequestViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(viewModel);
            await SetAvailableDaysAsync(viewModel);
            return View(viewModel);
        }

        // Validate date range
        if (viewModel.EndDate < viewModel.StartDate)
        {
            ModelState.AddModelError(nameof(viewModel.EndDate), "End date must be greater than or equal to start date.");
            await PopulateDropdownsAsync(viewModel);
            await SetAvailableDaysAsync(viewModel);
            return View(viewModel);
        }

        try
        {
            var leaveRequestDto = new DTOs.LeaveRequestDto
            {
                EmployeeId = viewModel.EmployeeId,
                LeaveTypeId = viewModel.LeaveTypeId,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                Reason = viewModel.Reason,
                Status = "Pending"
            };

            await _leaveService.CreateLeaveRequestAsync(leaveRequestDto);
            TempData["SuccessMessage"] = "Leave request submitted successfully.";
            return RedirectToAction(nameof(History));
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdownsAsync(viewModel);
            await SetAvailableDaysAsync(viewModel);
            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdownsAsync(viewModel);
            await SetAvailableDaysAsync(viewModel);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave request");
            TempData["ErrorMessage"] = "An error occurred while submitting the leave request.";
            await PopulateDropdownsAsync(viewModel);
            await SetAvailableDaysAsync(viewModel);
            return View(viewModel);
        }
    }

    // GET: Leave/List
    public async Task<IActionResult> List(string? status)
    {
        try
        {
            IEnumerable<DTOs.LeaveRequestDto> leaveRequests;

            if (!string.IsNullOrEmpty(status))
            {
                leaveRequests = await _leaveService.GetLeaveRequestsByStatusAsync(status);
            }
            else
            {
                leaveRequests = await _leaveService.GetAllLeaveRequestsAsync();
            }

            var viewModels = leaveRequests.Select(lr => new LeaveRequestListViewModel
            {
                LeaveRequestId = lr.LeaveRequestId,
                EmployeeId = lr.EmployeeId,
                EmployeeName = lr.EmployeeName,
                EmpCode = lr.EmpCode,
                LeaveTypeName = lr.LeaveTypeName,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                Reason = lr.Reason,
                Status = lr.Status,
                TotalDays = lr.RequestedDays,
                RemainingDays = lr.RemainingDays
            }).ToList();

            var currentStatus = status ?? string.Empty;
            ViewBag.StatusFilter = currentStatus;
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem("All Status", "", string.IsNullOrWhiteSpace(currentStatus)),
                new SelectListItem("Pending", "Pending", string.Equals(currentStatus, "Pending", StringComparison.OrdinalIgnoreCase)),
                new SelectListItem("Approved", "Approved", string.Equals(currentStatus, "Approved", StringComparison.OrdinalIgnoreCase)),
                new SelectListItem("Rejected", "Rejected", string.Equals(currentStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
            };

            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave requests list");
            TempData["ErrorMessage"] = "An error occurred while retrieving leave requests.";
            return View(new List<LeaveRequestListViewModel>());
        }
    }

    // GET: Leave/History
    public async Task<IActionResult> History(int? employeeId)
    {
        try
        {
            IEnumerable<DTOs.LeaveRequestDto> leaveRequests;

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                leaveRequests = await _leaveService.GetLeaveRequestsByEmployeeIdAsync(employeeId.Value);
            }
            else
            {
                // If no employee ID provided, get all (or could redirect to List)
                leaveRequests = await _leaveService.GetAllLeaveRequestsAsync();
            }

            var viewModels = leaveRequests.Select(lr => new LeaveRequestListViewModel
            {
                LeaveRequestId = lr.LeaveRequestId,
                EmployeeId = lr.EmployeeId,
                EmployeeName = lr.EmployeeName,
                EmpCode = lr.EmpCode,
                LeaveTypeName = lr.LeaveTypeName,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                Reason = lr.Reason,
                Status = lr.Status,
                TotalDays = lr.RequestedDays,
                RemainingDays = lr.RemainingDays
            }).ToList();

            ViewBag.EmployeeFilter = employeeId;
            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave history");
            TempData["ErrorMessage"] = "An error occurred while retrieving leave history.";
            return View(new List<LeaveRequestListViewModel>());
        }
    }

    // POST: Leave/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        try
        {
            await _leaveService.UpdateLeaveRequestStatusAsync(id, "Approved");
            TempData["SuccessMessage"] = "Leave request approved successfully.";
            return RedirectToAction(nameof(List));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(List));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving leave request, ID {LeaveRequestId}", id);
            TempData["ErrorMessage"] = "An error occurred while approving the leave request.";
            return RedirectToAction(nameof(List));
        }
    }

    // POST: Leave/Reject/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        try
        {
            await _leaveService.UpdateLeaveRequestStatusAsync(id, "Rejected");
            TempData["SuccessMessage"] = "Leave request rejected successfully.";
            return RedirectToAction(nameof(List));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(List));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting leave request, ID {LeaveRequestId}", id);
            TempData["ErrorMessage"] = "An error occurred while rejecting the leave request.";
            return RedirectToAction(nameof(List));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableDays(int employeeId, int leaveTypeId)
    {
        var allocation = await _leaveAllocationRepository.GetAllocationAsync(employeeId, leaveTypeId);
        return Json(new { remainingDays = allocation?.RemainingDays ?? 0 });
    }

    // Helper method to populate dropdown lists
    private async Task PopulateDropdownsAsync(LeaveRequestViewModel viewModel)
    {
        try
        {
            // Populate Employees dropdown
            var employees = await _employeeService.GetAllEmployeesAsync();
            viewModel.Employees = employees.Select(e => new EmployeeDropdownViewModel
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                EmpCode = e.EmpCode
            }).ToList();

            // Populate LeaveTypes dropdown
            var leaveTypes = await _leaveTypeRepository.GetAllAsync();
            viewModel.LeaveTypes = leaveTypes.Select(lt => new LeaveTypeDropdownViewModel
            {
                LeaveTypeId = lt.LeaveTypeId,
                LeaveName = lt.LeaveName
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating dropdowns");
        }
    }

    private async Task SetAvailableDaysAsync(LeaveRequestViewModel viewModel)
    {
        if (viewModel.EmployeeId > 0 && viewModel.LeaveTypeId > 0)
        {
            var allocation = await _leaveAllocationRepository.GetAllocationAsync(viewModel.EmployeeId, viewModel.LeaveTypeId);
            viewModel.AvailableDays = allocation?.RemainingDays ?? 0;
        }
        else
        {
            viewModel.AvailableDays = 0;
        }
    }
}

