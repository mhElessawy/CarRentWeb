namespace CarRentWeb.Models.MyModel;

public class SteppedAdminTaskViewModel
{
    public int Id { get; set; }
    public string TaskTitle { get; set; } = null!;
    public string? AssignedTo { get; set; }
    public string? Notes { get; set; }
    public List<StepInput> Steps { get; set; } = new();

    public class StepInput
    {
        public int Id { get; set; }
        public string StepName { get; set; } = null!;
        public string TargetDate { get; set; } = null!;
        public string? Notes { get; set; }
        public int StepOrder { get; set; }
    }
}
