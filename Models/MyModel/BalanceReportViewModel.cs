namespace CarRentWeb.Models.MyModel
{
    public class BalanceReportViewModel
    {
        public decimal TotalDailyRentPaid { get; set; }
        public decimal TotalMonthlyRentPaid { get; set; }
        public decimal TotalRentPaid => TotalDailyRentPaid + TotalMonthlyRentPaid;
        public decimal TotalDebtsPaid { get; set; }
        public decimal GrandTotal => TotalRentPaid + TotalDebtsPaid;
    }
}
