namespace CarRentWeb.Models;

public class PeriodicTaskInstanceStep
{
    public int Id { get; set; }
    public int InstanceId { get; set; }
    public int StepId { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateOnly? CompletedDate { get; set; }

    public virtual PeriodicTaskInstance? Instance { get; set; }
    public virtual PeriodicTaskStep? Step { get; set; }
}