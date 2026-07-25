namespace CarRentWeb.Models.MyModel
{
    public class AccountStatementContractItem
    {
        public string? ContractNo { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CarNo { get; set; }
        public decimal? DailyCredit { get; set; }
        public decimal? TotalCost { get; set; }
        public int? Status { get; set; }
        public decimal TotalRent { get; set; }
        public decimal PaidRent { get; set; }
        public decimal RemainingRent => TotalRent - PaidRent;
    }

    public class AccountStatementRow
    {
        public int EmpCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? CompanyName { get; set; }
        public DateOnly TxnDate { get; set; }
        public decimal Rent { get; set; }
        public decimal Debt { get; set; }
        public decimal RentPaid { get; set; }
        public decimal DebtPaid { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public class AccountStatementViewModel
    {
        public bool HasSearched { get; set; }

        public string? EmployeeName { get; set; }
        public int? EmpCode { get; set; }
        public string? MobileNo { get; set; }
        public string? CompanyName { get; set; }

        public List<AccountStatementContractItem> Contracts { get; set; } = new();
        public decimal TotalRent => Contracts.Sum(c => c.TotalRent);
        public decimal PaidRent => Contracts.Sum(c => c.PaidRent);
        public decimal RemainingRent => TotalRent - PaidRent;

        public decimal TotalDebt { get; set; }
        public decimal PaidDebt { get; set; }
        public decimal RemainingDebt { get; set; }

        public List<AccountStatementRow> Rows { get; set; } = new();
        public decimal TotalRentInPeriod => Rows.Sum(r => r.Rent);
        public decimal TotalDebtInPeriod => Rows.Sum(r => r.Debt);
        public decimal TotalRentPaidInPeriod => Rows.Sum(r => r.RentPaid);
        public decimal TotalDebtPaidInPeriod => Rows.Sum(r => r.DebtPaid);
    }
}
