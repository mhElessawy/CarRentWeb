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
    public string? Location { get; set; }
    public string? Jeha { get; set; }
}

public class EmployeeScheduleItem
{
    public int EmployeeId { get; set; }
    public string FullNameAr { get; set; } = "";
    public int? EmpCode { get; set; }
    public string? CompanyName { get; set; }
    public int NextStepId { get; set; }
    public string NextStepName { get; set; } = "";
    public string? NextStepJeha { get; set; }
    public string? NextStepLocation { get; set; }
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
    public DateOnly? TargetDate { get; set; }
    public int? ProgressRecordId { get; set; }
}