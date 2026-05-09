using CarRentWeb.Models;
using NPOI.HWPF;
using NPOI.HWPF.UserModel;
using NPOI.XWPF.UserModel;
using NPOI.OpenXmlFormats.Wordprocessing;
using System.IO;

namespace CarRentWeb.Service;

public class ContractDocService
{
    private readonly IWebHostEnvironment _env;

    public ContractDocService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Opens ContractNewEn.doc (HWPF binary), converts it to XWPF (.docx) in memory,
    /// finds the Second Party signature cell, and inserts the employee's stamp image.
    /// Returns the modified file as a byte array in .docx format.
    /// </summary>
    public byte[] GenerateWithStamp(Contract contract)
    {
        var templatePath = Path.Combine(_env.ContentRootPath, "ContractNewEn.doc");

        // ── 1. Load the binary .doc template ──────────────────────────────────────
        HWPFDocument hwpf;
        using (var fs = File.OpenRead(templatePath))
            hwpf = new HWPFDocument(fs);

        // ── 2. Convert HWPF → XWPF so we can work with the modern Open XML API ───
        var xwpf = new XWPFDocument();
        ConvertHwpfToXwpf(hwpf, xwpf);

        // ── 3. Insert stamp into the last table's Second Party cell ───────────────
        var emp = contract.Employee;
        if (!string.IsNullOrEmpty(emp?.StampImagePath))
        {
            var stampPath = Path.Combine(_env.WebRootPath,
                                         emp.StampImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(stampPath))
                InsertStampInSecondPartyCell(xwpf, stampPath);
        }

        // ── 4. Serialise to bytes ─────────────────────────────────────────────────
        using var ms = new MemoryStream();
        xwpf.Write(ms);
        return ms.ToArray();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private static void ConvertHwpfToXwpf(HWPFDocument hwpf, XWPFDocument xwpf)
    {
        var range = hwpf.GetRange();

        for (int pi = 0; pi < range.NumParagraphs; pi++)
        {
            var hwpfPara = range.GetParagraph(pi);

            // Check if inside a table cell
            if (hwpfPara.IsInTable())
            {
                // Tables are handled separately; skip here (we rebuild them below)
                continue;
            }

            var xwpfPara = xwpf.CreateParagraph();
            var run      = xwpfPara.CreateRun();
            run.SetText(hwpfPara.Text.TrimEnd('\r', '\n'));
        }

        // Rebuild tables
        var tables = hwpf.GetRange().GetTables();
        foreach (var hwpfTable in tables)
        {
            var xwpfTable = xwpf.CreateTable(hwpfTable.NumRows, hwpfTable.GetRow(0).NumCells());
            xwpfTable.SetInsideHBorder(XWPFTable.XWPFBorderType.SINGLE, 1, 0, "000000");
            xwpfTable.SetInsideVBorder(XWPFTable.XWPFBorderType.SINGLE, 1, 0, "000000");

            for (int ri = 0; ri < hwpfTable.NumRows; ri++)
            {
                var hwpfRow = hwpfTable.GetRow(ri);
                for (int ci = 0; ci < hwpfRow.NumCells(); ci++)
                {
                    var hwpfCell  = hwpfRow.GetCell(ci);
                    var xwpfCell  = xwpfTable.GetRow(ri).GetCell(ci);

                    // Collect text from all paragraphs in the cell
                    string cellText = "";
                    for (int pci = 0; pci < hwpfCell.NumParagraphs; pci++)
                        cellText += hwpfCell.GetParagraph(pci).Text.TrimEnd('\r', '\n', '\a') + " ";

                    xwpfCell.SetText(cellText.Trim());
                }
            }
        }
    }

    private static void InsertStampInSecondPartyCell(XWPFDocument doc, string stampPath)
    {
        var tables = doc.Tables;
        if (tables == null || tables.Count == 0) return;

        // The signature table is the LAST table in the document
        var signTable = tables[tables.Count - 1];
        XWPFTableCell? secondPartyCell = null;

        // Walk all cells to find "Second Party"
        foreach (var row in signTable.Rows)
        {
            foreach (var cell in row.GetTableCells())
            {
                if (cell.GetText().Contains("Second Party", StringComparison.OrdinalIgnoreCase))
                {
                    secondPartyCell = cell;
                    break;
                }
            }
            if (secondPartyCell != null) break;
        }

        // Fallback: use last row, second cell
        if (secondPartyCell == null)
        {
            var lastRow = signTable.Rows[signTable.Rows.Count - 1];
            var cells   = lastRow.GetTableCells();
            if (cells.Count >= 2)
                secondPartyCell = cells[cells.Count - 1];
        }

        if (secondPartyCell == null) return;

        // Determine image type
        var ext = Path.GetExtension(stampPath).ToLower();
        var picType = ext switch
        {
            ".png"  => PictureType.PNG,
            ".gif"  => PictureType.GIF,
            ".bmp"  => PictureType.BMP,
            _       => PictureType.JPEG
        };

        // Add a new paragraph to the cell and insert the image
        var stampPara = secondPartyCell.AddParagraph();
        stampPara.Alignment = ParagraphAlignment.CENTER;
        var stampRun = stampPara.CreateRun();

        using var imgStream = File.OpenRead(stampPath);
        // Width ~130pt, Height ~70pt (in EMUs: 1pt = 12700 EMU)
        stampRun.AddPicture(imgStream, (int)picType, Path.GetFileName(stampPath),
                             Units.ToEMU(130), Units.ToEMU(70));
    }
}
