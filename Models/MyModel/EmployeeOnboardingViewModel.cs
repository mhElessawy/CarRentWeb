using CarRentWeb.Models;

namespace CarRentWeb.Models.MyModel;

public class EmployeeOnboardingViewModel
{
    public EmployeeInfo Employee { get; set; } = new();
    public List<StepProgressItem> Steps { get; set; } = new();
    public int CompletedCount => Steps.Count(s => s.IsCompleted);
    public int TotalCount => Steps.Count;
}

public class StepProgressItem
{
    public int StepId { get; set; }
    public string StepName { get; set; } = "";
    public string? Description { get; set; }
    public int StepOrder { get; set; }
    public bool IsCompleted { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public string? Notes { get; set; }
}
