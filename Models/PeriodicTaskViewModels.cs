namespace CarRentWeb.Models;

public class ActiveTaskViewModel
{
    public PeriodicTaskInstance Instance { get; set; } = null!;
    public PeriodicTaskDef Def { get; set; } = null!;
    public string EntityName { get; set; } = "";
    public DateOnly DueDate { get; set; }
    public int DaysLeft { get; set; }
    public string FieldLabel { get; set; } = "";
}

public class PeriodicScheduleItem
{
    public int InstanceId { get; set; }
    public string TaskName { get; set; } = "";
    public string SourceType { get; set; } = "";
    public string EntityName { get; set; } = "";
    public DateOnly DueDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public int DaysLeft { get; set; }
    public string FieldLabel { get; set; } = "";
}
