namespace CarRentWeb.Models;

public class PeriodicTaskStep
{
    public int Id { get; set; }
    public int TaskDefId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = null!;
    public string? Authority { get; set; }
    public string? Location { get; set; }

    public virtual PeriodicTaskDef TaskDef { get; set; } = null!;
    public virtual ICollection<PeriodicTaskInstanceStep> InstanceSteps { get; set; } = new List<PeriodicTaskInstanceStep>();
}
