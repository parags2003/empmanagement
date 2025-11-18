using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.Repository;
using EmployeeLeaveManagement.Services;
using EmployeeLeaveManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.Controllers;

public class LeaveAllocationController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IRepository<LeaveType> _leaveTypeRepository;
    private readonly ILeaveAllocationRepository _leaveAllocationRepository;
    private readonly ILogger<LeaveAllocationController> _logger;

    public LeaveAllocationController(
        IEmployeeService employeeService,
        IRepository<LeaveType> leaveTypeRepository,
        ILeaveAllocationRepository leaveAllocationRepository,
        ILogger<LeaveAllocationController> logger)
    {
        _employeeService = employeeService;
        _leaveTypeRepository = leaveTypeRepository;
        _leaveAllocationRepository = leaveAllocationRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? employeeId)
    {
        var viewModel = await BuildViewModelAsync(employeeId);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(LeaveAllocationViewModel model)
    {
        if (model.SelectedEmployeeId <= 0)
        {
            ModelState.AddModelError(nameof(model.SelectedEmployeeId), "Please select an employee.");
        }

        if (model.SelectedLeaveTypeId <= 0)
        {
            ModelState.AddModelError(nameof(model.SelectedLeaveTypeId), "Please select a leave type.");
        }

        if (!ModelState.IsValid)
        {
            var vm = await BuildViewModelAsync(model.SelectedEmployeeId);
            vm.SelectedLeaveTypeId = model.SelectedLeaveTypeId;
            vm.TotalDays = model.TotalDays;
            return View("Index", vm);
        }

        try
        {
            var allocation = await _leaveAllocationRepository.GetAllocationAsync(model.SelectedEmployeeId, model.SelectedLeaveTypeId);
            if (allocation == null)
            {
                allocation = new EmployeeLeaveAllocation
                {
                    EmployeeId = model.SelectedEmployeeId,
                    LeaveTypeId = model.SelectedLeaveTypeId,
                    TotalDays = model.TotalDays,
                    UsedDays = 0
                };
                await _leaveAllocationRepository.AddAsync(allocation);
            }
            else
            {
                allocation.TotalDays = model.TotalDays;
                if (allocation.UsedDays > model.TotalDays)
                {
                    allocation.UsedDays = model.TotalDays;
                }

                await _leaveAllocationRepository.UpdateAsync(allocation);
            }

            TempData["SuccessMessage"] = "Leave allocation saved successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving leave allocation");
            TempData["ErrorMessage"] = "An error occurred while saving the leave allocation.";
        }

        return RedirectToAction(nameof(Index), new { employeeId = model.SelectedEmployeeId });
    }

    private async Task<LeaveAllocationViewModel> BuildViewModelAsync(int? employeeId)
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        var leaveTypes = await _leaveTypeRepository.GetAllAsync();

        var selectedEmployeeId = employeeId ?? employees.FirstOrDefault()?.EmployeeId ?? 0;
        var selectedLeaveTypeId = leaveTypes.FirstOrDefault()?.LeaveTypeId ?? 0;

        var viewModel = new LeaveAllocationViewModel
        {
            SelectedEmployeeId = selectedEmployeeId,
            SelectedLeaveTypeId = selectedLeaveTypeId,
            Employees = employees.Select(e => new EmployeeDropdownViewModel
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                EmpCode = e.EmpCode
            }).ToList(),
            LeaveTypes = leaveTypes.Select(lt => new LeaveTypeDropdownViewModel
            {
                LeaveTypeId = lt.LeaveTypeId,
                LeaveName = lt.LeaveName
            }).ToList()
        };

        if (viewModel.SelectedEmployeeId > 0)
        {
            var allocations = await _leaveAllocationRepository.GetByEmployeeAsync(viewModel.SelectedEmployeeId);
            viewModel.Allocations = allocations.Select(a => new LeaveAllocationItemViewModel
            {
                LeaveTypeName = a.LeaveType.LeaveName,
                TotalDays = a.TotalDays,
                UsedDays = a.UsedDays,
                RemainingDays = a.RemainingDays
            }).ToList();
        }

        if (viewModel.SelectedLeaveTypeId > 0 && viewModel.SelectedEmployeeId > 0)
        {
            var allocation = await _leaveAllocationRepository.GetAllocationAsync(viewModel.SelectedEmployeeId, viewModel.SelectedLeaveTypeId);
            if (allocation != null)
            {
                viewModel.TotalDays = allocation.TotalDays;
                viewModel.CurrentBalance = allocation.RemainingDays;
            }
            else
            {
                var defaultDays = leaveTypes.FirstOrDefault(lt => lt.LeaveTypeId == viewModel.SelectedLeaveTypeId)?.DefaultDays ?? 0;
                viewModel.TotalDays = defaultDays;
                viewModel.CurrentBalance = defaultDays;
            }
        }

        return viewModel;
    }
}

