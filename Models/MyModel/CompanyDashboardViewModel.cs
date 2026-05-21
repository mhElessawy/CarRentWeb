namespace CarRentWeb.Models.MyModel;

public class CompanyDashboardItem
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = "";
    public int? MoiAllowedCars { get; set; }
    public int ActualCars { get; set; }
    public int ActiveContracts { get; set; }
    public double? Ratio => MoiAllowedCars > 0
        ? Math.Round((double)ActiveContracts / MoiAllowedCars.Value * 100, 1)
        : null;
}
