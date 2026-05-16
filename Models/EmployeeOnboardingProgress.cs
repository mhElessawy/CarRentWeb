namespace CarRentWeb.Models;

public class EmployeeOnboardingProgress
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int StepId { get; set; }
    public bool IsCompleted { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public string? Notes { get; set; }

    public virtual EmployeeInfo? Employee { get; set; }
    public virtual DriverOnboardingStep? Step { get; set; }
}
