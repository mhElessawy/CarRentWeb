namespace CarRentWeb.Models.MyModel
{
    public class ContractDetailsSumation
    {
        public int EmployeeId { get; set; }
        public int EmpCode {  get; set; }
        public string? MobileNo {  get; set; }
        public string? EmployeeName { get; set; }
        public string? CompanyName { get; set; }
        public decimal TotalDailyCredit { get; set; }   // الإيجار المدفوع
        public decimal RemainingRent { get; set; }       // الباقي من العقد
        public decimal TotalCarCredit { get; set; }      // الديون المدفوعة
        public decimal RemainingCarCredit { get; set; }  // الديون الباقية
        public decimal RemainingDebt { get; set; }       // إجمالي الباقي (used in BalanceReport)
        public decimal OverdueRental { get; set; }       // إجمالي المتأخير
    }
    public class CarInfoWithCreditsViewModel
    {
        public CarInfo Car { get; set; }
        public decimal PayedCredit { get; set; }
        public decimal RemainingCredit { get; set; }
    }
}
