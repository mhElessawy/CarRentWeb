namespace CarRentWeb.Models.MyModel
{
    public class AccountStatementItem
    {
        public DateOnly Date { get; set; }
        public string Description { get; set; } = "";
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
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

        public decimal TotalDebit => Items.Sum(i => i.Debit);
        public decimal TotalCredit => Items.Sum(i => i.Credit);
        public decimal ClosingBalance => TotalDebit - TotalCredit;
    }
}
