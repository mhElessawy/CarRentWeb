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
    /// Loads ContractNewEn.doc (original binary .doc) and fills in contract data
    /// using HWPFDocument.Range.ReplaceText — no conversion, formatting preserved.
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

        var templatePath = Path.Combine(_env.ContentRootPath, "ContractNewEn.doc");

        HWPFDocument hwpf;
        using (var fs = File.OpenRead(templatePath))
            hwpf = new HWPFDocument(fs);

        var range = hwpf.GetRange();

        // Preamble – contract date
        // Template has double-space gaps: "On  corresponding to  the present contract..."
        range.ReplaceText("On  corresponding to  the", $"On {cDate} the");

        // Preamble – company name and field
        range.ReplaceText("facility  entitled", $"facility entitled {compName}");
        range.ReplaceText("working in the field of  whereas", "working in the field of Car Rental, whereas");
        range.ReplaceText("profession of  whereas", "profession of Car Driver, whereas");

        // First party (company) – table cell labels followed by blank spaces
        range.ReplaceText("1.Company ", $"1.Company {compName} ");
        range.ReplaceText("File No / ", $"File No: {fileNo} / ");
        range.ReplaceText("Civil license number  / ", $"Civil license No: {licNo} / ");

        // Second party (employee) – table cell labels followed by blank spaces
        range.ReplaceText("Name: ", $"Name: {empName} ");
        range.ReplaceText("Nationality: ", $"Nationality: {natName} ");
        range.ReplaceText("Civil card:", $"Civil card: {civilId}");
        if (!string.IsNullOrWhiteSpace(address))
            range.ReplaceText(" Residence:", $" Residence: {address}");

        // Article Two – profession blank
        range.ReplaceText("profession of   in the State", "profession of Car Driver in the State");

        // Article Three – fill blank paragraphs after heading
        TryFillArticleThree(range, startDate, endDate, noOfDays);

        // Article Four – wage blank (double-space gap)
        range.ReplaceText("the wage of  dinars", $"the wage of {wage} KWD");

        // Article Five – start date blank
        range.ReplaceText("come into force on  The second party",
            $"come into force on {startDate}. The second party");

        // Article Six – start date + duration blanks
        range.ReplaceText("come into force on  for a term of   years",
            $"come into force on {startDate} and shall end on {endDate} ({noOfDays} days)");

        using var ms = new MemoryStream();
        hwpf.Write(ms);
        return ms.ToArray();
    }

    // Article Three's body is blank paragraphs in the template.
    // Insert the contract-term sentence before the first blank paragraph after the heading.
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
                if (string.IsNullOrWhiteSpace(text))
                {
                    range.GetParagraph(i).InsertBefore(
                        $"Contract term: from {startDate} to {endDate} ({noOfDays} days).");
                    break;
                }
                break; // hit non-blank content before finding a blank paragraph
            }
        }
        catch { /* InsertBefore may not be supported; silently skip */ }
    }
}
