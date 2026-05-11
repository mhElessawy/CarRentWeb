namespace CarRentWeb.Models.MyModel
{
    public class BalanceReportViewModel
    {
        public int EmpId { get; set; }
        public int EmpCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? MobileNo { get; set; }
        public decimal OverdueRental { get; set; }
        public decimal OverdueDebit { get; set; }
        public decimal Total => OverdueRental + OverdueDebit;
    }
}
