using CarRentWeb.Data;
using CarRentWeb.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;

public class WordDocumentService
{
    private readonly CarRentWebContext _context;

    public WordDocumentService(CarRentWebContext context)
    {
        _context = context;
    }

    public byte[] GeneratePermitDocument(int employeeId)
    {
        return GenerateDocument(employeeId, "NewPerm.docx");
    }

    public byte[] GenerateRenewalDocument(int employeeId)
    {
        return GenerateDocument(employeeId, "ReNewPermSp.docx");
    }

    private byte[] GenerateDocument(int employeeId, string templateFileName)
    {
        // Get employee with related data
        var employee = _context.EmployeeInfos
            .Include(e => e.Nationality)
            .Include(e => e.JobTitle)
            .Include(e => e.Company)
            .FirstOrDefault(e => e.Id == employeeId);

        if (employee == null)
            throw new Exception("Employee not found");

        // Path to template
        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateFileName);

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Template file not found", templatePath);

        // Create temporary file with correct extension
        string extension = Path.GetExtension(templateFileName);
        string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
        File.Copy(templatePath, tempFilePath, true);

        try
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(tempFilePath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;

                // Update bookmarks
                UpdateBookmarks(doc, employee);

                // Also update any text placeholders
                UpdateTextPlaceholders(body, employee);

                doc.Save();
            }

            return File.ReadAllBytes(tempFilePath);
        }
        finally
        {
            // Clean up
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    private void UpdateBookmarks(WordprocessingDocument doc, EmployeeInfo employee)
    {
        var body = doc.MainDocumentPart.Document.Body;

        // Get all bookmarks
        var bookmarkStarts = body.Descendants<BookmarkStart>().ToList();

        foreach (var bookmarkStart in bookmarkStarts)
        {
            string bookmarkName = bookmarkStart.Name;
            string value = GetBookmarkValue(bookmarkName, employee);

            if (!string.IsNullOrEmpty(value))
            {
                // Find the text to replace
                ReplaceBookmarkText(bookmarkStart, value);
            }
        }
    }

    private void ReplaceBookmarkText(BookmarkStart bookmarkStart, string newValue)
    {
        // Find bookmark end
        var bookmarkEnd = FindBookmarkEnd(bookmarkStart);

        if (bookmarkEnd == null)
            return;

        // Look for text between bookmark start and end
        var current = bookmarkStart.NextSibling();
        while (current != null && current != bookmarkEnd)
        {
            if (current is Run run)
            {
                var textElement = run.GetFirstChild<Text>();
                if (textElement != null)
                {
                    textElement.Text = newValue;
                    return;
                }
            }
            current = current.NextSibling();
        }

        // If no text element found, insert one
        if (bookmarkStart.Parent != null)
        {
            var newRun = new Run(new Text(newValue));
            bookmarkStart.Parent.InsertAfter(newRun, bookmarkStart);
        }
    }

    private BookmarkEnd FindBookmarkEnd(BookmarkStart bookmarkStart)
    {
        var body = bookmarkStart.Ancestors<Body>().FirstOrDefault();
        if (body == null)
            return null;

        var bookmarkEnds = body.Descendants<BookmarkEnd>().ToList();
        return bookmarkEnds.FirstOrDefault(be => be.Id == bookmarkStart.Id);
    }

    private string GetBookmarkValue(string bookmarkName, EmployeeInfo employee)
    {
        // Map bookmark names to employee properties
        switch (bookmarkName)
        {
            case "ResNo3":
            case "ResNo2":
            case "ResNo4":
            case "ResNo5":
            case "CivilId":
            case "LicenseNo":
            case "LicenseNo2":
                return employee.CivilId ?? "";
            case "FirstName":
            case "FirstName2":
                return employee.FirstNameAr!;
            case "SecondName2":
            case "SecondName":
                return employee.SecondNameAr!;
            case "ThirdName":
            case "ThirdName2":
                return employee.ThirdNameAr!;
            case "FourthName":
            case "FourthName2":
                return employee.ForthNameAr!;
            case "LastName":
            case "LastName2":
                return employee.LastNameAr!;
            case "ArName":
            case "FullArName3":
                return employee.FullNameAr!;
            case "NameEn":
            case "Name":
                return employee.FullNameEn ?? "";
            case "Nationality":
            case "Nationality2":
            case "Nationality3":
            case "Nationality5":
                return employee.Nationality?.DeffName ?? "";

            case "Gender":
            case "الجنس":
                return employee.Gender == 1 ? "ذكر" : "أنثى";

            case "JobTitle":
            case "JobTitle2":
            case "JobTitle3":
                return employee.JobTitle?.DeffName ?? "";

            case "BirthDate":
            case "تاريخ_الميلاد":
            case "تاريخ الميلاد":
                return employee.EmpBirthDate?.ToString("dd/MM/yyyy") ?? "";

            case "Address":
            case "عنوان":
            case "عنوان السكن":
                return employee.EmpAddress ?? "";
            case "WorkAddress":
            case "عنوان العمل":
                return employee.Company?.CompNameAr ?? "";

            case "EmpCode":
            case "كود":
                return employee.EmpCode?.ToString() ?? "";



            case "LicenseType":
            case "نوع_الرخصة":
            case "نوع الرخصة":
                return "خصوصي";

            case "LicenseNationality":
            case "جنسية_الرخصة":
            case "جنسيتها":
                return "الكويت";

            case "EmpStartLicence":
            case "تاريخ_الإصدار":
            case "تاريخ الاصدار":
                return employee.StartLicense?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");

            case "ExpiryDate":
            case "EmpEndLicence":
            case "تاريخ الانتهاء":
                return employee.EndLicense?.ToString("dd/MM/yyyy") ?? DateTime.Now.AddYears(1).ToString("dd/MM/yyyy");

            case "FileNumber":
            case "رقم_الملف":
            case "رقم الملف":
                return "7890";

            case "CurrentDate":
            case "التاريخ":
                return DateTime.Now.ToString("dd/MM/yyyy");

            case "RequestDate":
            case "تاريخ_الطلب":
            case "تاريخ الطلب":
                return DateTime.Now.ToString("dd/MM/yyyy");

            case "RequestType":
            case "نوع_الطلب":
            case "نوع الطلب":
                return "تجديد تصريح إجرة جوالة";

            case "Fees":
            case "الرسوم":
                return "د. كويتي";

            case "Phone":
            case "هاتف":
            case "TelNo":
                return employee.TelNo ?? "";

            case "Mobile":
            case "جوال":
            case "MobiileNo":
                return employee.MobiileNo ?? "";

            case "PassportNo":
            case "رقم_الجواز":
                return employee.PassportNo ?? "";

            case "BloodType":
            case "فصيلة_الدم":
            case "فصيلة الدم":
                return "O+"; // Default or add to employee model
            case "CompanyName":
                return employee.Company!.OwnerName1 ?? "";
            case "TraficLocationName":
                return "";
            default:
                return null;
        }
    }

    private void UpdateTextPlaceholders(Body body, EmployeeInfo employee)
    {
        var texts = body.Descendants<Text>().ToList();

        foreach (var text in texts)
        {
            string originalText = text.Text;

            // Replace common placeholders
            if (originalText.Contains("[CivilId]") || originalText.Contains("{{CivilId}}"))
                text.Text = originalText.Replace("[CivilId]", employee.CivilId ?? "")
                                       .Replace("{{CivilId}}", employee.CivilId ?? "");

            if (originalText.Contains("[NameAr]") || originalText.Contains("{{NameAr}}"))
                text.Text = originalText.Replace("[NameAr]", employee.FullNameAr ?? "")
                                       .Replace("{{NameAr}}", employee.FullNameAr ?? "");

            if (originalText.Contains("[NameEn]") || originalText.Contains("{{NameEn}}"))
                text.Text = originalText.Replace("[NameEn]", employee.FullNameEn ?? "")
                                       .Replace("{{NameEn}}", employee.FullNameEn ?? "");

            if (originalText.Contains("[Nationality]") || originalText.Contains("{{Nationality}}"))
                text.Text = originalText.Replace("[Nationality]", employee.Nationality?.DeffName ?? "")
                                       .Replace("{{Nationality}}", employee.Nationality?.DeffName ?? "");

            // Replace placeholder lines
            if (originalText.Contains("---") || originalText.Contains("...") || originalText.Contains("______"))
                text.Text = "";

            // Replace date placeholders
            if (originalText.Contains("[Date]") || originalText.Contains("{{Date}}"))
                text.Text = originalText.Replace("[Date]", DateTime.Now.ToString("dd/MM/yyyy"))
                                       .Replace("{{Date}}", DateTime.Now.ToString("dd/MM/yyyy"));
        }
    }

    public byte[] GenerateEnglishContractDocument(int contractId)
    {
        var contract = _context.Contracts
            .Include(c => c.Employee).ThenInclude(e => e!.Nationality)
            .Include(c => c.Employee).ThenInclude(e => e!.JobTitle)
            .Include(c => c.Employee).ThenInclude(e => e!.Company)
            .FirstOrDefault(c => c.Id == contractId);

        if (contract == null)
            throw new Exception("Contract not found");

        var emp = contract.Employee;
        var company = emp?.Company;

        string companyName = company?.CompNameEn ?? company?.CompNameAr ?? "";
        string companyNameAr = company?.CompNameAr ?? companyName;
        string fileNo = company?.ManpowerFileNo ?? company?.CompFileNo ?? "";
        string licenseNo = company?.CompLicenseNo ?? "";
        string empNameEn = emp?.FullNameEn ?? emp?.FullNameAr ?? "";
        string empNameAr = emp?.FullNameAr ?? empNameEn;
        string nationality = emp?.Nationality?.DeffName ?? "";
        string civilId = emp?.CivilId ?? "";
        string address = emp?.EmpAddress ?? "";
        string jobTitle = emp?.JobTitle?.DeffName ?? "";
        string salary = emp?.Salary?.ToString("F3") ?? "";
        string startDate = contract.StartDate?.ToString("dd/MM/yyyy") ?? "";
        string contractDate = contract.ContractDate?.ToString("dd/MM/yyyy") ?? "";
        int years = 1;
        if (contract.StartDate.HasValue && contract.EndDate.HasValue)
            years = Math.Max(1, (int)Math.Round(
                (contract.EndDate.Value.ToDateTime(TimeOnly.MinValue)
                - contract.StartDate.Value.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25));

        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "ContractNewEn.docx");
        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Contract template not found", templatePath);

        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.docx");
        File.Copy(templatePath, tempFile, true);

        try
        {
            using (var doc = WordprocessingDocument.Open(tempFile, true))
            {
                var body = doc.MainDocumentPart!.Document.Body!;

                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    // Arabic bookmarks
                    ["ArContractDate"]  = contractDate,
                    ["ArArabicDate"]    = contractDate,
                    ["ArCompanyName"]   = companyNameAr,
                    ["ArFileNo"]        = fileNo,
                    ["ArLicenseNo"]     = licenseNo,
                    ["ArEmpName"]       = empNameAr,
                    ["ArNationality"]   = nationality,
                    ["ArCivilId"]       = civilId,
                    ["ArAddress"]       = address,
                    ["ArFacilityName"]  = companyNameAr,
                    ["ArWorkField"]     = "تأجير سيارات الأجرة",
                    ["ArProfession1"]   = jobTitle,
                    ["ArProfession2"]   = jobTitle,
                    ["ArSalary"]        = salary,
                    ["ArStartDate1"]    = startDate,
                    ["ArStartDate2"]    = startDate,
                    ["ArYears"]         = years.ToString(),
                    // English bookmarks
                    ["EnContractDate"]  = contractDate,
                    ["EnArabicDate"]    = contractDate,
                    ["EnCompanyName"]   = companyName,
                    ["EnFileNo"]        = fileNo,
                    ["EnLicenseNo"]     = licenseNo,
                    ["EnEmpName"]       = empNameEn,
                    ["EnNationality"]   = nationality,
                    ["EnCivilId"]       = civilId,
                    ["EnAddress"]       = address,
                    ["EnFacilityName"]  = companyName,
                    ["EnWorkField"]     = "Car Rental",
                    ["EnProfession1"]   = jobTitle,
                    ["EnProfession2"]   = jobTitle,
                    ["EnSalary"]        = salary,
                    ["EnStartDate1"]    = startDate,
                    ["EnStartDate2"]    = startDate,
                    ["EnYears"]         = years.ToString(),
                };

                foreach (var bk in body.Descendants<BookmarkStart>().ToList())
                {
                    if (bk.Name != null && values.TryGetValue(bk.Name, out var val))
                        ReplaceBookmarkText(bk, val);
                }

                doc.Save();
            }
            return File.ReadAllBytes(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }



    // Method to list all bookmarks in template (for debugging)
    public List<string> GetTemplateBookmarks()
    {
        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "NewPerm.docx");

        if (!File.Exists(templatePath))
            return new List<string> { "Template file not found" };

        var bookmarks = new List<string>();

        using (WordprocessingDocument doc = WordprocessingDocument.Open(templatePath, false))
        {
            var body = doc.MainDocumentPart.Document.Body;
            var bookmarkStarts = body.Descendants<BookmarkStart>().ToList();

            foreach (var bookmark in bookmarkStarts)
            {
                bookmarks.Add(bookmark.Name);
            }
        }

        return bookmarks;
    }
}
