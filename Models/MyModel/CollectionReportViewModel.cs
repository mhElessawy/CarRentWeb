namespace CarRentWeb.Models.MyModel
{
    public class BillCollectionItem
    {
        public int BillNo { get; set; }
        public DateOnly? BillDate { get; set; }
        public int EmpCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? CompanyName { get; set; }
        public string? ContractNo { get; set; }
        public string? CarNo { get; set; }
        public decimal Amount { get; set; }
    }

    public class DebitPayCollectionItem
    {
        public int DebitPayNo { get; set; }
        public DateOnly? PayDate { get; set; }
        public int EmpCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? CompanyName { get; set; }
        public string? DebitType { get; set; }
        public decimal Amount { get; set; }
    }

    public class CollectionReportViewModel
    {
        public List<BillCollectionItem> Bills { get; set; } = new();
        public List<DebitPayCollectionItem> DebitPayments { get; set; } = new();
        public decimal TotalBills => Bills.Sum(b => b.Amount);
        public decimal TotalDebitPayments => DebitPayments.Sum(d => d.Amount);
        public decimal GrandTotal => TotalBills + TotalDebitPayments;
    }
}