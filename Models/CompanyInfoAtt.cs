using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CarRentWeb.Models;

public partial class CompanyInfoAtt
{
    public int Id { get; set; }
    public int CompId { get; set; }
    public required string TitleData { get; set; }

    public string? PathFileData { get; set; }

    [NotMapped]
    [ValidateNever]
    public IFormFile? pdfFile1 { get; set; }

    [ValidateNever]
    public virtual CompanyInfo? Comp { get; set; }
}
