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
        string fileNo = company?.ManpowerFileNo ?? company?.CompFileNo ?? "";
        string licenseNo = company?.CompLicenseNo ?? "";
        string empName = emp?.FullNameEn ?? emp?.FullNameAr ?? "";
        string nationality = emp?.Nationality?.DeffName ?? "";
        string civilId = emp?.CivilId ?? "";
        string address = emp?.EmpAddress ?? "";
        string jobTitle = emp?.JobTitle?.DeffName ?? "";
        string salary = emp?.Salary?.ToString("F3") ?? "";
        string startDate = contract.StartDate?.ToString("dd/MM/yyyy") ?? "";
        string contractDate = contract.ContractDate?.ToString("dd/MM/yyyy") ?? "";
        int years = 1;
        if (contract.StartDate.HasValue && contract.EndDate.HasValue)
            years = Math.Max(1, (int)Math.Round((contract.EndDate.Value.ToDateTime(TimeOnly.MinValue) - contract.StartDate.Value.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25));

        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();
            mainPart.Document.AppendChild(body);

            // Page margins
            var sectProps = new SectionProperties(
                new PageMargin { Top = 720, Bottom = 720, Left = 900, Right = 900 });
            body.AppendChild(sectProps);

            // Header
            body.InsertBefore(EcParagraph("Public Authority for Manpower                    Labour Department", bold: true, center: true, size: 28), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Date
            body.InsertBefore(EcParagraph($"On  {contractDate}  corresponding to  the present contract was concluded by and between:", size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // First party
            body.InsertBefore(EcParagraph($"1. Company:  {companyName}", size: 22), sectProps);
            body.InsertBefore(EcParagraph($"   File No / {fileNo}          Civil license number / {licenseNo}", size: 22), sectProps);
            body.InsertBefore(EcParagraph("                                                                      (First party)", size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Second party
            body.InsertBefore(EcParagraph($"   Name:  {empName}", size: 22), sectProps);
            body.InsertBefore(EcParagraph($"   Nationality:  {nationality}", size: 22), sectProps);
            body.InsertBefore(EcParagraph($"   Civil card:  {civilId}          Residence:  {address}", size: 22), sectProps);
            body.InsertBefore(EcParagraph("                                                                      (Second party)", size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Preamble
            body.InsertBefore(EcParagraph("Preamble", bold: true, size: 24), sectProps);
            body.InsertBefore(EcParagraph(
                $"The first party owns the facility entitled  {companyName}  working in the field of car rental " +
                $"whereas it wishes to conclude a contract with the second party to work for it in the profession of  {jobTitle}.",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);
            body.InsertBefore(EcParagraph(
                "whereas acknowledged  their  capacity  to   conclude  this contract, they agreed upon the following:",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Article One
            body.InsertBefore(EcParagraph("Article One", bold: true, size: 24), sectProps);
            body.InsertBefore(EcParagraph("The preamble above shall constitute an integral part of the present contract.", size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Article Two
            body.InsertBefore(EcParagraph("Article Two - Nature of the Work", bold: true, size: 24), sectProps);
            body.InsertBefore(EcParagraph(
                $"The first party concluded a contract with the second party to work for it in the profession of  {jobTitle}  in the State of Kuwait.",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Article Three
            body.InsertBefore(EcParagraph("Article Three", bold: true, size: 24), sectProps);
            body.InsertBefore(EcParagraph(
                "Considering the contract as having a definite or indefinite term shall be subject to the will of the two parties.",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Article Four
            body.InsertBefore(EcParagraph("Article Four - Lease Value", bold: true, size: 24), sectProps);
            body.InsertBefore(EcParagraph(
                $"For executing the present contract, the second party shall receive the wage of  {salary}  dinars " +
                "to be paid at the end of every month. The first party may not decrease the wage during the term of the contract. " +
                "It may not transfer the second party to daily wage without his approval.",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Article Five
            body.InsertBefore(EcParagraph("Article Five - Contract Term", bold: true, size: 24), sectProps);
            body.InsertBefore(EcParagraph(
                $"The contract shall come into force on  {startDate}.  The second party shall execute his work during the entire execution term thereof.",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Article Six
            body.InsertBefore(EcParagraph("Article Six - Contract Term", bold: true, size: 24), sectProps);
            body.InsertBefore(EcParagraph(
                $"The present contract has a definite term. It shall come into force on  {startDate}  for a term of  {years}  year(s).",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Article Seven
            body.InsertBefore(EcParagraph("Article Seven - Annual Leave", bold: true, size: 24), sectProps);
            body.InsertBefore(EcParagraph(
                "The second party shall have the right to a paid annual leave with a term of 30 days. " +
                "It shall not be due on the first year save after the expiration of nine months to be calculated from the date of the contract coming into force.",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Article Eight
            body.InsertBefore(EcParagraph("Article Eight - Number of Work Hours", bold: true, size: 24), sectProps);
            body.InsertBefore(EcParagraph(
                "The first party may not require that the second party work for a term exceeding eight daily work hours with rest periods not less than one hour.",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);

            // Signatures
            body.InsertBefore(EcParagraph("", size: 20), sectProps);
            body.InsertBefore(EcParagraph(
                "First Party Signature                                                Second Party Signature",
                size: 22), sectProps);
            body.InsertBefore(EcParagraph("", size: 20), sectProps);
            body.InsertBefore(EcParagraph(
                "_______________________                                         _______________________",
                size: 22), sectProps);

            doc.Save();
        }
        return ms.ToArray();
    }

    private static Paragraph EcParagraph(string text, bool bold = false, bool center = false, int size = 22)
    {
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        if (center)
            pPr.AppendChild(new Justification { Val = JustificationValues.Center });
        pPr.AppendChild(new SpacingBetweenLines { After = "80" });
        para.AppendChild(pPr);

        var run = new Run();
        var rPr = new RunProperties();
        if (bold) rPr.AppendChild(new Bold());
        rPr.AppendChild(new FontSize { Val = size.ToString() });
        rPr.AppendChild(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
        run.AppendChild(rPr);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        para.AppendChild(run);
        return para;
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
