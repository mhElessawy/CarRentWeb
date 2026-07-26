namespace CarRentWeb.Models.MyModel
{
    public class AccountStatementItem
    {
        public int EmpCode { get; set; }
        public string EmployeeName { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public DateOnly Date { get; set; }
        public decimal Rent { get; set; }
        public decimal Debt { get; set; }
        public decimal RentPay { get; set; }
        public decimal DebtPay { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public class AccountStatementViewModel
    {
        public bool HasEmployee { get; set; }
        public int EmpCode { get; set; }
        public string EmployeeName { get; set; } = "";
        public string MobileNo { get; set; } = "";
        public string CompanyName { get; set; } = "";

        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }

        public List<AccountStatementItem> Items { get; set; } = new();

        public decimal TotalRent => Items.Sum(i => i.Rent);
        public decimal TotalDebt => Items.Sum(i => i.Debt);
        public decimal TotalRentPay => Items.Sum(i => i.RentPay);
        public decimal TotalDebtPay => Items.Sum(i => i.DebtPay);
        public decimal ClosingBalance => Items.Any() ? Items.Last().CurrentBalance : 0;
    }
}
