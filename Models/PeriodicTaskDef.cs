namespace CarRentWeb.Models;

public class PeriodicTaskDef
{
    public int Id { get; set; }
    public string TaskName { get; set; } = null!;
    public string SourceType { get; set; } = null!; // "Employee" or "Car"
    public string DateFieldName { get; set; } = null!;
    public int AlertDaysBefore { get; set; } = 30;
    public bool IsActive { get; set; } = true;

    public virtual ICollection<PeriodicTaskStep> Steps { get; set; } = new List<PeriodicTaskStep>();
    public virtual ICollection<PeriodicTaskInstance> Instances { get; set; } = new List<PeriodicTaskInstance>();
}
