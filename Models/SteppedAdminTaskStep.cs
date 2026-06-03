namespace CarRentWeb.Models;

public class SteppedAdminTaskStep
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = null!;
    public DateOnly TargetDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateOnly? CompletedDate { get; set; }
    public string? Notes { get; set; }

    public virtual SteppedAdminTask Task { get; set; } = null!;
}
