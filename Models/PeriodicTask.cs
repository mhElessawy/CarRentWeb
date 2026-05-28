namespace CarRentWeb.Models;

public class PeriodicTask
{
    public int Id { get; set; }
    public string TaskName { get; set; } = "";
    public string? Description { get; set; }
    public int TaskOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Location { get; set; }
    public string? Jeha { get; set; }
    public string? Frequency { get; set; }

    public virtual ICollection<PeriodicTaskInstanceStep> InstanceSteps { get; set; } = new List<PeriodicTaskInstanceStep>();
}
