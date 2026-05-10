using CarRentWeb.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CarRentWeb.Service;

public class ContractDocService
{
    private readonly IWebHostEnvironment _env;

    public ContractDocService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Copies ContractNewEn.docx, fills every named bookmark with contract data
    /// using DocumentFormat.OpenXml (no third-party libraries), and returns the
    /// modified .docx bytes.  The original template file is never modified.
    /// </summary>
    public byte[] GenerateWithData(Contract contract)
    {
        var emp     = contract.Employee;
        var company = emp?.Company;

        string cDate     = contract.ContractDate?.ToString("dd/MM/yyyy") ?? "";
        string startDate = contract.StartDate?.ToString("dd/MM/yyyy") ?? "";
        string endDate   = contract.EndDate?.ToString("dd/MM/yyyy") ?? "";
        string noOfDays  = contract.NoOfDays?.ToString() ?? "";
        string compEn    = company?.CompNameEn ?? company?.CompNameAr ?? "";
        string compAr    = company?.CompNameAr ?? company?.CompNameEn ?? "";
        string fileNo    = company?.CompFileNo ?? "";
        string licNo     = company?.CompLicenseNo ?? "";
        string owner     = company?.OwnerName1 ?? "";
        string empEn     = emp?.FullNameEn ?? emp?.FullNameAr ?? "";
        string empAr     = emp?.FullNameAr ?? emp?.FullNameEn ?? "";
        string nat       = emp?.Nationality?.DeffName ?? "";
        string civilId   = emp?.CivilId ?? "";
        string address   = emp?.EmpAddress ?? "";
        string wage      = contract.DailyCredit?.ToString("F0") ?? "";

        var templatePath = Path.Combine(_env.WebRootPath, "Templates", "ContractNewEn.docx");
        var templateBytes = File.ReadAllBytes(templatePath);

        using var ms = new MemoryStream();
        ms.Write(templateBytes, 0, templateBytes.Length);

        using (var doc = WordprocessingDocument.Open(ms, true))
        {
            var body = doc.MainDocumentPart!.Document.Body!;

            // ── English bookmarks ──────────────────────────────────────────────
            Fill(body, "CompNameEng",         compEn);
            Fill(body, "CompNameEng1",        compEn);
            Fill(body, "CompFileNoEn",        fileNo);
            Fill(body, "CompOwnerEng",        owner);
            Fill(body, "CompOwnerEng1",       owner);
            Fill(body, "CompOwnerCivilIDEng", licNo);
            Fill(body, "CompActivateEng",     "Car Rental");
            Fill(body, "EmpNameEng",          empEn);
            Fill(body, "EmpNameEng1",         empEn);
            Fill(body, "EmpNationalityEng",   nat);
            Fill(body, "EmpCivilIDEng",       civilId);
            Fill(body, "EmpResidenceEng",     address);
            Fill(body, "EmpJobTitleEng",      "Car Driver");
            Fill(body, "EmpJobTitleEng1",     "Car Driver");
            Fill(body, "EmpSalaryEng",        wage);
            Fill(body, "ContractDateEng1",    startDate);
            Fill(body, "ContractStartDateEng",startDate);
            Fill(body, "ContractPeriodEng",   noOfDays);

            // ── Arabic bookmarks ───────────────────────────────────────────────
            Fill(body, "CompNameAr",          compAr);
            Fill(body, "CompNameAr1",         compAr);
            Fill(body, "CompFileNoAr",        fileNo);
            Fill(body, "CompOwnerAr",         owner);
            Fill(body, "CompOwnerAr1",        owner);
            Fill(body, "CompOwnerCivilIDAr",  licNo);
            Fill(body, "CompActivateAr",      "تأجير سيارات");
            Fill(body, "EmpNameAr",           empAr);
            Fill(body, "EmpNameAr1",          empAr);
            Fill(body, "EmpNationalityAr",    nat);
            Fill(body, "EmpCivilIDAr",        civilId);
            Fill(body, "EmpResidenceAr",      address);
            Fill(body, "EmpJobTitleAr",       "سائق");
            Fill(body, "EmpJobTitleAr1",      "سائق");
            Fill(body, "EmpSalaryAr",         wage);
            Fill(body, "EmpSalarTafketAr",    wage);
            Fill(body, "ContractDayAr",       cDate);
            Fill(body, "ContractDateAr",      cDate);
            Fill(body, "ContractDateAr1",     cDate);
            Fill(body, "ContractStartDateAr", startDate);
            Fill(body, "ContractPeriodAr",    noOfDays);

            // ── Preamble date has no English bookmark – replace the text run ──
            ReplaceText(body, "On  corresponding to  the", $"On {cDate} the");

            doc.MainDocumentPart.Document.Save();
        }

        return ms.ToArray();
    }

    // Insert value as a new run immediately after the named bookmark start.
    // Copies run formatting from the nearest sibling run so the inserted text
    // matches the surrounding font/size.
    private static void Fill(Body body, string bookmarkName, string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        var start = body.Descendants<BookmarkStart>()
                        .FirstOrDefault(b => b.Name?.Value == bookmarkName);
        if (start == null) return;

        // Clone run properties from a sibling run for consistent formatting
        var sibRun = start.Parent?.Descendants<Run>().FirstOrDefault();
        var rPr    = sibRun?.RunProperties?.CloneNode(true) as RunProperties;

        var run = new Run(new Text(value) { Space = SpaceProcessingModeValues.Preserve });
        if (rPr != null) run.InsertAt(rPr, 0);

        start.InsertAfterSelf(run);
    }

    // Direct text-node replacement for runs that have no bookmark.
    private static void ReplaceText(Body body, string search, string replacement)
    {
        foreach (var t in body.Descendants<Text>())
            if (t.Text.Contains(search))
                t.Text = t.Text.Replace(search, replacement);
    }
}
