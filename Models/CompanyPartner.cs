using System.ComponentModel.DataAnnotations;

namespace CarRentWeb.Models;

public class CompanyPartner
{
    public int Id { get; set; }

    [Display(Name = "الشركة")]
    public int CompId { get; set; }

    [Display(Name = "اسم الشريك")]
    public string? PartnerName { get; set; }

    [Display(Name = "الجنسية")]
    public string? Nationality { get; set; }

    [Display(Name = "الصفة")]
    public string? Role { get; set; }

    [Display(Name = "حصة الشريك %")]
    public decimal? SharePercentage { get; set; }

    public virtual CompanyInfo? Comp { get; set; }
}
