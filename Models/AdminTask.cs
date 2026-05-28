namespace CarRentWeb.Models;

public class AdminTask
{
    public int Id { get; set; }
    public string TaskTitle { get; set; } = null!;
    public string? Notes { get; set; }
    public DateOnly TargetDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateOnly? CompletedDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; }
}