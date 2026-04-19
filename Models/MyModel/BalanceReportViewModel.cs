namespace CarRentWeb.Models.MyModel
{
    public class BalanceReportItemViewModel
    {
        public int EmpCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? MobileNo { get; set; }
        public string? CompanyName { get; set; }
        public decimal OverdueRental { get; set; }
        public decimal RemainingDebt { get; set; }
    }

    public class BalanceReportViewModel
    {
        public List<BalanceReportItemViewModel> Items { get; set; } = new();
        public decimal TotalOverdueRental => Items.Sum(i => i.OverdueRental);
        public decimal TotalRemainingDebt => Items.Sum(i => i.RemainingDebt);
        public decimal GrandTotal => TotalOverdueRental + TotalRemainingDebt;
    }
}