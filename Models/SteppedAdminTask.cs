namespace CarRentWeb.Models;

public class SteppedAdminTask
{
    public int Id { get; set; }
    public string TaskTitle { get; set; } = null!;
    public string? AssignedTo { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateOnly? CompletedDate { get; set; }

    public virtual ICollection<SteppedAdminTaskStep> Steps { get; set; } = new List<SteppedAdminTaskStep>();
}
