namespace CarRentWeb.Models.MyModel
{
    public class BalanceReportViewModel
    {
        public int EmployeeId { get; set; }
        public int EmpCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? MobileNo { get; set; }
        public string? CompanyName { get; set; }
        public decimal OverdueRental { get; set; }   // الرصيد المتأخر من الإيجار (ContractDetails)
        public decimal RemainingDebt { get; set; }   // الديون المتبقية (DebitInfo)
        public decimal GrandTotal => OverdueRental + RemainingDebt;
    }
}
