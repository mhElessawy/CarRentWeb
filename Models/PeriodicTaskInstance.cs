namespace CarRentWeb.Models;

public class PeriodicTaskInstance
{
    public int Id { get; set; }
    public int TaskDefId { get; set; }
    public int SourceId { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsCompleted { get; set; } = false;

    public virtual PeriodicTaskDef? TaskDef { get; set; }
    public virtual ICollection<PeriodicTaskInstanceStep> StepStatuses { get; set; } = new List<PeriodicTaskInstanceStep>();
}
