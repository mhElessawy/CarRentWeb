using CarRentWeb.Models;
using NPOI.HWPF;
using NPOI.HWPF.UserModel;

namespace CarRentWeb.Service;

public class ContractDocService
{
    private readonly IWebHostEnvironment _env;

    public ContractDocService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Loads ContractNewEn.doc, fills in contract data via HWPF text replacement,
    /// and returns the modified binary .doc bytes.  Formatting is preserved because
    /// we work inside the binary OLE2 format without any conversion to XWPF.
    /// </summary>
    public byte[] GenerateWithData(Contract contract)
    {
        var emp     = contract.Employee;
        var company = emp?.Company;

        string cDate     = contract.ContractDate?.ToString("dd/MM/yyyy") ?? "";
        string startDate = contract.StartDate?.ToString("dd/MM/yyyy") ?? "";
        string endDate   = contract.EndDate?.ToString("dd/MM/yyyy") ?? "";
        string noOfDays  = contract.NoOfDays?.ToString() ?? "";
        string compName  = company?.CompNameEn ?? company?.CompNameAr ?? "";
        string fileNo    = company?.CompFileNo ?? "";
        string licNo     = company?.CompLicenseNo ?? "";
        string empName   = emp?.FullNameEn ?? emp?.FullNameAr ?? "";
        string natName   = emp?.Nationality?.DeffName ?? "";
        string civilId   = emp?.CivilId ?? "";
        string address   = emp?.EmpAddress ?? "";
        string wage      = contract.DailyCredit?.ToString("F0") ?? "";
        string total     = contract.TotalCost?.ToString("F0") ?? "";

        var templatePath = Path.Combine(_env.ContentRootPath, "ContractNewEn.doc");

        HWPFDocument hwpf;
        using (var fs = File.OpenRead(templatePath))
            hwpf = new HWPFDocument(fs);

        var range = hwpf.GetRange();

        // ── Preamble ──────────────────────────────────────────────────────────
        // Template: "On  corresponding to  the present contract..."
        // Blank = double spaces (Arabic date, Gregorian date)
        range.ReplaceText("On  corresponding to  the", $"On {cDate} the");

        // Template: "facility  entitled     working in the field of  whereas"
        range.ReplaceText("facility  entitled", $"facility entitled {compName}");
        range.ReplaceText("working in the field of  whereas", "working in the field of Car Rental, whereas");
        range.ReplaceText("profession of  whereas", "profession of Car Driver, whereas");

        // ── First Party (Company) ─────────────────────────────────────────────
        // Each label is followed by blank space to end of cell paragraph
        range.ReplaceText("1.Company ", $"1.Company {compName} ");
        range.ReplaceText("File No / ", $"File No: {fileNo} / ");
        range.ReplaceText("Civil license number  / ", $"Civil license No: {licNo} / ");

        // ── Second Party (Employee) ───────────────────────────────────────────
        range.ReplaceText("Name: ", $"Name: {empName} ");
        range.ReplaceText("Nationality: ", $"Nationality: {natName} ");
        range.ReplaceText("Civil card:", $"Civil card: {civilId}");
        if (!string.IsNullOrWhiteSpace(address))
            range.ReplaceText(" Residence:", $" Residence: {address}");

        // ── Article Two – Nature of the Work ──────────────────────────────────
        // Template: "profession of   in the State of Kuwait"
        range.ReplaceText("profession of   in the State", "profession of Car Driver in the State");

        // ── Article Three – Contract Term (blank paragraphs) ─────────────────
        TryFillArticleThree(range, startDate, endDate, noOfDays);

        // ── Article Four – Lease Value ────────────────────────────────────────
        // Template: "the wage of  dinars"
        range.ReplaceText("the wage of  dinars", $"the wage of {wage} KWD");

        // ── Article Five – Contract Term (start date) ─────────────────────────
        // Template: "come into force on  The second party shall execute"
        range.ReplaceText("come into force on  The second party",
            $"come into force on {startDate}. The second party");

        // ── Article Six – Contract Term (definite, start + duration) ──────────
        // Template: "come into force on  for a term of   years."
        range.ReplaceText("come into force on  for a term of   years",
            $"come into force on {startDate} and shall end on {endDate} ({noOfDays} days)");

        using var ms = new MemoryStream();
        hwpf.Write(ms);
        return ms.ToArray();
    }

    // Article Three's body consists of blank paragraphs in the template.
    // We insert the contract-term text as a new paragraph before the first blank one.
    private static void TryFillArticleThree(Range range, string startDate, string endDate, string noOfDays)
    {
        try
        {
            bool found = false;
            for (int i = 0; i < range.NumParagraphs; i++)
            {
                string text = range.GetParagraph(i).Text.Trim();

                if (!found)
                {
                    if (text.Equals("Article Three", StringComparison.OrdinalIgnoreCase))
                        found = true;
                    continue;
                }

                // First blank paragraph after the heading
                if (string.IsNullOrWhiteSpace(text))
                {
                    range.GetParagraph(i).InsertBefore(
                        $"Contract term: from {startDate} to {endDate} ({noOfDays} days).");
                    break;
                }

                // Hit non-blank content before finding a blank – stop
                break;
            }
        }
        catch { /* InsertBefore may not be supported in all NPOI builds; silently skip */ }
    }
}
