using Microsoft.AspNetCore.Http;
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

    [Display(Name = "رقم عقد الإيجار")]
    public string? RentContractNo { get; set; }

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
    [Display(Name = " الرقم الآلي للعنوان")]
    public string? AddressAutoNo { get; set; }
    [Display(Name = "الشارع")]
    public string? AddressStreet { get; set; }
    [Display(Name = "اسم المبنى")]
    public string? AddressBuildingName { get; set; }
    [Display(Name = "رقم الوحدة")]
    public string? AddressUnitNo { get; set; }
    [Display(Name = " الإيجار")]
    [Precision(18, 2)]
    public decimal? AddressRent { get; set; }
    [Display(Name = " بداية الإيجار")]
    public DateOnly? StartRent { get; set; }
    
    [Display(Name = " نهاية الإيجار")]
    public DateOnly? EndRent { get; set; }

    //    end

    //  بيانات معلومات مدنيه

    [Display(Name = "تاريخ المعلومات المدنية")]
    public DateOnly? CivilInfoDate { get; set; }
    [Display(Name = "المرجع")]
    public string? CivilInfoRef { get; set; }

    [Display(Name = "اسم اللافته")]
    public string? CivilSignboardName { get; set; }

    [Display(Name = "اسم الرخصه")]
    public string? CivilLicenseName { get; set; }

    [Display(Name = "اسم الحائز")]
    public string? CivilLicenseHolder { get; set; }

    [Display(Name = "الاسم اللاتيني")]
    public string? CivilLatinName { get; set; }

    [Display(Name = "الرقم المدني للجهة")]
    public string? CivilID { get; set; }

    [Display(Name = "النشاط الإقتصادي")]
    public string? CivilEconomicActivity { get; set; }

    [Display(Name = "جهة الترخيص")]
    public string? CivilLicenseAuthority { get; set; }

    [Display(Name = "رقم ملف جهة الترخيص")]
    public string? CompFileNo { get; set; }
    [Display(Name = "رقم الرخصه")]
    public string? CompLicenseNo { get; set; }


    // وزارة التجارة والصناعة
    [Display(Name = "رقم الترخيص")]
    public string? TradeLicenseNo { get; set; }

    [Display(Name = "تاريخ بداية الترخيص")]
    public DateOnly? StartLicense { get; set; }
    [Display(Name = "تاريخ نهاية الترخيص")]
    public DateOnly? EndLicense { get; set; }
    [Display(Name = "الرقم المركزي")]
    public string? CenterNo { get; set; }
    [Display(Name = "رقم السجل التجاري")]
    public int? CommercialRegistrationNo { get; set; }

    [Display(Name = "المحافظة")]
    public int? LocationId { get; set; }

    [Display(Name = "حالة التفعيل")]
    public int? CompActivateId { get; set; }

    [Display(Name = "المدينة")]
    public int? CityId { get; set; }

    //  end

    // رخصه تجاريه+مستخرج

    [Display(Name = "الكيان القانوني")]
    public string? LegalEntity { get; set; }

    [Display(Name = "نوع الترخيص")]
    public string? LicenseType { get; set; }

    [Display(Name = "الاسم التجاري")]
    public string? TradeName { get; set; }

    [Display(Name = "رأس المال")]
    [Precision(18, 2)]
    public decimal? Capital { get; set; }

    [Display(Name = "تاريخ القيد في السجل")]
    public DateOnly? RegistrationDate { get; set; }

    [Display(Name = "رمز النشاط")]
    public string? ActivityCode { get; set; }

    [Display(Name = "حالة الشركة")]
    public string? CompanyStatus { get; set; }

    [Display(Name = "عدد الشركاء")]
    public int? PartnersCount { get; set; }

    [Display(Name = "عدد الأنشطة")]
    public int? ActivitiesCount { get; set; }

    [Display(Name = "عدد المديرين")]
    public int? ManagersCount { get; set; }

    [Display(Name = "حالة الترخيص الرئيسي")]
    public string? MainLicenseStatus { get; set; }

    [Display(Name = "بداية السنة المالية (شهر)")]
    public int? FiscalYearStartMonth { get; set; }

    [Display(Name = "نهاية السنة المالية (شهر)")]
    public int? FiscalYearEndMonth { get; set; }

    [Display(Name = "صلاحيات المدير")]
    public string? ManagerPermissions { get; set; }

    // end

    // شهادة سداد التأمينات

    [Display(Name = "رقم تسجيل التأمينات")]
    public string? InsuranceRegNo { get; set; }

    [Display(Name = "القطاع")]
    public string? InsuranceSector { get; set; }

    [Display(Name = "مرجع شهادة التأمينات")]
    public string? InsuranceRef { get; set; }

    [Display(Name = "تاريخ شهادة التأمينات")]
    public DateOnly? InsuranceCertDate { get; set; }

    [Display(Name = "صلاحية شهادة التأمينات")]
    public DateOnly? InsuranceCertValidDate { get; set; }

    // end

    // اعتماد توقيع - القوى العاملة

    [Display(Name = "رقم ملف القوى العاملة")]
    public string? ManpowerFileNo { get; set; }

    [Display(Name = "نوع الملف")]
    public string? ManpowerFileType { get; set; }

    [Display(Name = "تصنيف الملف")]
    public string? ManpowerFileClassification { get; set; }

    [Display(Name = "إدارة العمل")]
    public string? ManpowerWorkAdmin { get; set; }

    [Display(Name = "اجمالي العقود الحكومية")]
    public int? GovContractsCount { get; set; }

    [Display(Name = "الرقم الموحد")]
    public string? UnifiedNo { get; set; }

    [Display(Name = "اسم المفوض")]
    public string? AuthorizedPersonName { get; set; }

    [Display(Name = "الرقم المدني للمفوض")]
    public string? AuthorizedPersonCivilId { get; set; }

    [Display(Name = "تاريخ التفويض")]
    public DateOnly? AuthorizationDate { get; set; }

    [Display(Name = "تاريخ إصدار شهادة القوى العاملة")]
    public DateOnly? ManpowerCertIssueDate { get; set; }

    [Display(Name = "تاريخ انتهاء شهادة القوى العاملة")]
    public DateOnly? ManpowerCertExpiryDate { get; set; }

    // end


    [Display(Name = "صورة توقيع الاعتماد")]
    public string? CompSignature { get; set; }

    [NotMapped]
    public IFormFile? SignatureFile { get; set; }

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

    public virtual ICollection<CompanyPartner> CompanyPartners { get; set; } = new List<CompanyPartner>();

    public virtual ICollection<CompanyApproval> CompanyApprovals { get; set; } = new List<CompanyApproval>();
}