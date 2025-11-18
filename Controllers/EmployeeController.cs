using System.Linq;
using EmployeeLeaveManagement.Services;
using EmployeeLeaveManagement.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.Controllers;

public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;
    private readonly IWebHostEnvironment _environment;

    public EmployeeController(
        IEmployeeService employeeService,
        ILogger<EmployeeController> logger,
        IWebHostEnvironment environment)
    {
        _employeeService = employeeService;
        _logger = logger;
        _environment = environment;
    }

    // GET: Employee
    public async Task<IActionResult> Index(string? searchTerm)
    {
        try
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim().ToLowerInvariant();
                employees = employees.Where(e =>
                    (!string.IsNullOrEmpty(e.Name) && e.Name.ToLowerInvariant().Contains(normalized)) ||
                    (!string.IsNullOrEmpty(e.EmpCode) && e.EmpCode.ToLowerInvariant().Contains(normalized)) ||
                    (!string.IsNullOrEmpty(e.Email) && e.Email.ToLowerInvariant().Contains(normalized)) ||
                    (!string.IsNullOrEmpty(e.Department) && e.Department.ToLowerInvariant().Contains(normalized)) ||
                    (!string.IsNullOrEmpty(e.Position) && e.Position.ToLowerInvariant().Contains(normalized))
                );
            }

            var viewModels = employees.Select(e => new EmployeeViewModel
            {
                EmployeeId = e.EmployeeId,
                EmpCode = e.EmpCode,
                Name = e.Name,
                Email = e.Email,
                Department = e.Department,
                Position = e.Position,
                PhotoPath = e.PhotoPath
            }).ToList();

            ViewBag.SearchTerm = searchTerm;
            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            TempData["ErrorMessage"] = "An error occurred while retrieving employees.";
            return View(new List<EmployeeViewModel>());
        }
    }

    // GET: Employee/Details/5
    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var viewModel = new EmployeeViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmpCode = employee.EmpCode,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                Position = employee.Position,
                PhotoPath = employee.PhotoPath
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee details for ID {EmployeeId}", id);
            TempData["ErrorMessage"] = "An error occurred while retrieving employee details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Employee/Create
    public IActionResult Create()
    {
        return View(new EmployeeViewModel());
    }

    // POST: Employee/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            // Check if employee code already exists
            if (await _employeeService.EmpCodeExistsAsync(viewModel.EmpCode))
            {
                ModelState.AddModelError(nameof(viewModel.EmpCode), "Employee code already exists.");
                return View(viewModel);
            }

            // Check if email already exists
            if (await _employeeService.EmailExistsAsync(viewModel.Email))
            {
                ModelState.AddModelError(nameof(viewModel.Email), "Email already exists.");
                return View(viewModel);
            }

            var photoPath = await SavePhotoAsync(viewModel.PhotoUpload, null);

            var employeeDto = new DTOs.EmployeeDto
            {
                EmpCode = viewModel.EmpCode,
                Name = viewModel.Name,
                Email = viewModel.Email,
                Department = viewModel.Department,
                Position = viewModel.Position,
                PhotoPath = photoPath
            };

            await _employeeService.CreateEmployeeAsync(employeeDto);
            TempData["SuccessMessage"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            TempData["ErrorMessage"] = "An error occurred while creating the employee.";
            return View(viewModel);
        }
    }

    // GET: Employee/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var viewModel = new EmployeeViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmpCode = employee.EmpCode,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                Position = employee.Position,
                PhotoPath = employee.PhotoPath
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee for edit, ID {EmployeeId}", id);
            TempData["ErrorMessage"] = "An error occurred while retrieving the employee.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Employee/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeViewModel viewModel)
    {
        if (id != viewModel.EmployeeId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            // Check if employee code already exists for another employee
            var existingEmployee = await _employeeService.GetEmployeeByEmpCodeAsync(viewModel.EmpCode);
            if (existingEmployee != null && existingEmployee.EmployeeId != id)
            {
                ModelState.AddModelError(nameof(viewModel.EmpCode), "Employee code already exists.");
                return View(viewModel);
            }

            // Check if email already exists for another employee
            var existingEmail = await _employeeService.GetEmployeeByEmailAsync(viewModel.Email);
            if (existingEmail != null && existingEmail.EmployeeId != id)
            {
                ModelState.AddModelError(nameof(viewModel.Email), "Email already exists.");
                return View(viewModel);
            }

            var photoPath = await SavePhotoAsync(viewModel.PhotoUpload, viewModel.PhotoPath);

            var employeeDto = new DTOs.EmployeeDto
            {
                EmployeeId = viewModel.EmployeeId,
                EmpCode = viewModel.EmpCode,
                Name = viewModel.Name,
                Email = viewModel.Email,
                Department = viewModel.Department,
                Position = viewModel.Position,
                PhotoPath = photoPath
            };

            await _employeeService.UpdateEmployeeAsync(id, employeeDto);
            TempData["SuccessMessage"] = "Employee updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee, ID {EmployeeId}", id);
            TempData["ErrorMessage"] = "An error occurred while updating the employee.";
            return View(viewModel);
        }
    }

    // GET: Employee/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var viewModel = new EmployeeViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmpCode = employee.EmpCode,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                Position = employee.Position,
                PhotoPath = employee.PhotoPath
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee for delete, ID {EmployeeId}", id);
            TempData["ErrorMessage"] = "An error occurred while retrieving the employee.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Employee/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            if (!result)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Employee deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee, ID {EmployeeId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the employee.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<string?> SavePhotoAsync(IFormFile? photoFile, string? existingPath)
    {
        if (photoFile == null || photoFile.Length <= 0)
        {
            return existingPath;
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photoFile.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await photoFile.CopyToAsync(stream);

        return $"/uploads/{fileName}";
    }
}

