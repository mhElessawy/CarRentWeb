namespace CarRentWeb.Models;

public class PeriodicTaskInstanceStep
{
    public int Id { get; set; }
    public int PeriodicTaskId { get; set; }
    public string StepName { get; set; } = "";
    public string? Description { get; set; }
    public int StepOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Location { get; set; }
    public string? Jeha { get; set; }

    public virtual PeriodicTask? PeriodicTask { get; set; }
}
