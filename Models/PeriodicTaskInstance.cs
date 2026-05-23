namespace CarRentWeb.Models;

public class PeriodicTaskInstance
{
    public int Id { get; set; }
    public int TaskDefId { get; set; }
    public int SourceId { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsCompleted { get; set; } = false;

    public virtual PeriodicTaskDef TaskDef { get; set; } = null!;
    public virtual ICollection<PeriodicTaskInstanceStep> StepStatuses { get; set; } = new List<PeriodicTaskInstanceStep>();
}
