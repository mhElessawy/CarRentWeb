using System.ComponentModel.DataAnnotations;

namespace CarRentWeb.Models;

public class CompanyApproval
{
    public int Id { get; set; }

    [Display(Name = "الشركة")]
    public int CompId { get; set; }

    [Display(Name = "اسم الجهة")]
    public string? AuthorityName { get; set; }

    [Display(Name = "رقم الموافقة")]
    public string? ApprovalNo { get; set; }

    [Display(Name = "تاريخ الموافقة")]
    public DateOnly? ApprovalDate { get; set; }

    public virtual CompanyInfo? Comp { get; set; }
}
