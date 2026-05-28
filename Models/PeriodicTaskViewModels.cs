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

public class PeriodicTaskReportItem
{
    public int InstanceId { get; set; }
    public string TaskName { get; set; } = "";
    public string SourceType { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string FieldLabel { get; set; } = "";
    public DateOnly DueDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public bool IsCompleted { get; set; }
    public int DaysLeft { get; set; }
    public int? DelayDays { get; set; }
}

public class UnifiedTaskReportItem
{
    public string TaskType { get; set; } = "";      // "Foundation" | "Periodic" | "Admin"
    public string TaskTypeName { get; set; } = "";  // تأسيسية | دورية | إدارية
    public string TaskName { get; set; } = "";
    public string EntityName { get; set; } = "";
    public DateOnly? TargetDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public bool IsCompleted { get; set; }
    public int? DelayDays { get; set; }             // positive = late, negative = days remaining
}
