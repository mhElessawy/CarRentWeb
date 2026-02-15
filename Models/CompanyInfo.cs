using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentWeb.Models;

public partial class CompanyInfo
{
    public int Id { get; set; }

    [Display(Name = "كود الشركه")]
    public int? CompCode { get; set; }
    [Display(Name = "اسم الشركه بالعربي")]
    public string? CompNameAr { get; set; }
    [Display(Name = "اسم الشركه بالإنجليزي")]
    public string? CompNameEn { get; set; }
    [Display(Name = "شعار الشركه")]
    public string? CompLogo { get; set; }
    [Display(Name = "اسم صاحب العمل")]
    public string? OwnerName1 { get; set; }
    [Display(Name = "الرقم المدني")]
    public string? OwnerCivilId1 { get; set; }
    [Display(Name = "اسم صاحب الشركه 2")]
    public string? OwnerName2 { get; set; }
    [Display(Name = "الرقم المدني 2")]
    public string? OwnerCivilId2 { get; set; }
    [Display(Name = "اسم صاحب الشركه 3")]
    public string? OwnerName3 { get; set; }
    [Display(Name = "الرقم المدني 3")]
    public string? OwnerCivilId3 { get; set; }

    //  بيانات عقد الإيجار 
    [Display(Name = " صاحب العقار")]
    public string? OwnerHome { get; set; }

    [Display(Name = " المنطقه")]
    public int? AddressLocation { get; set; }
    [Display(Name = " القطعه")]
    public string? AddressArea { get; set; }
    [Display(Name = " القسيمه")]
    public string? AddressQasima { get; set; }
    [Display(Name = " الدور")]
    public string? AddressLevel { get; set; }
    [Display(Name = " نوع المبنى")]
    public int? AddressType { get; set; }
    [Display(Name = " رقم المبني")]
    public int? AddressTypeNo { get; set; }
    [Display(Name = " الرقم الآلي")]
    public string ? AddressAutoNo { get; set; }
    [Display(Name = " الإيجار")]
    [Precision(18, 2)]
    public decimal AddressRent { get; set; }
    [Display(Name = " بداية الإيجار")]
    public DateOnly StartRent { get; set; }
    [Display(Name = " نهاية الإيجار")]
    public DateOnly EndRent { get; set; }

   //    end 

    //  بيانات معلومات مدنيه 
    public string? CivilID { get; set; }

    [Display(Name = "تاريخ بداية الترخيص")]
    public int? CompActivateId { get; set; }
    [Display(Name = "رقم الملف")]
    public string? CompFileNo { get; set; }

    [Display(Name = "رقم الرخصه")]
    public string? CompLicenseNo { get; set; }


    //  end

    // رخصه تجاريه+مستخرج

    [Display(Name = "تاريخ بداية الترخيص")]
    public DateOnly StartLicense { get; set; }
    [Display(Name = "تاريخ نهاية الترخيص")]
    public DateOnly EndLicense { get; set; }

    [Display(Name = "الرقم المركزي")]
    public string? CenterNo { get; set; }
    [Display(Name = "رقم السجل التجاري")]
    public int CommercialRegistrationNo { get; set; }

    // end

    [Display(Name = "المحافظة")]
    public int? LocationId { get; set; }




    [Display(Name = "رقم الهاتف 1")]
    public string? Tel1 { get; set; }
    [Display(Name = "رقم الهاتف 2")]
    public string? Tel2 { get; set; }

    public DateOnly? CompReleaseDate { get; set; }

    public int? DeleteFlag { get; set; }

    public virtual ICollection<CarInfo> CarInfos { get; set; } = new List<CarInfo>();

    public virtual Deff? City { get; set; }

    public virtual Deff? CompActivate { get; set; }

    public virtual ICollection<CompanyInfoAtt> CompanyInfoAtts { get; set; } = new List<CompanyInfoAtt>();

    public virtual ICollection<EmployeeInfo> EmployeeInfos { get; set; } = new List<EmployeeInfo>();

    public virtual Deff? Location { get; set; }

    public virtual ICollection<UserCompanyNotAppear> UserCompanyNotAppears { get; set; } = new List<UserCompanyNotAppear>();
}
