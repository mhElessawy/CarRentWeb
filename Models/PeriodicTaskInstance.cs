namespace CarRentWeb.Models;

public class PeriodicTaskInstance
{
    public int Id { get; set; }
    public int PeriodicTaskId { get; set; }
    public int? EmployeeId { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public string? Notes { get; set; }
    public DateOnly CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public virtual PeriodicTask? TaskDef { get; set; }
    public virtual EmployeeInfo? Employee { get; set; }
    public virtual ICollection<PeriodicTaskInstanceStep> StepStatuses { get; set; } = new List<PeriodicTaskInstanceStep>();
}
