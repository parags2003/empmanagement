namespace EmployeeLeaveManagement.ViewModels;

public class DashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int OnLeave { get; set; }
    public int NewHires { get; set; }
    public List<RecentEmployeeViewModel> RecentEmployees { get; set; } = new();
}

public class RecentEmployeeViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

