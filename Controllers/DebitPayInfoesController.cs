using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using CarRentWeb.Data;
using CarRentWeb.Models;


namespace CarRentWeb.Controllers
{
    public class DebitPayInfoesController : Controller
    {
        private readonly CarRentWebContext _context;

        public DebitPayInfoesController(CarRentWebContext context)
        {
            _context = context;
        }

        // GET: DebitPayInfoes
        public async Task<IActionResult> Index(int? EmpCodeString, string? EmpSearch, int? DefTypeId, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            // Get companies for dropdown
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            //// Get the user's company data from TempData
            //// Get the user's company data from TempData
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            if (companyIds.Any())
            {
                ViewBag.Companies = new SelectList(
                            await _context.CompanyInfos
                                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                                .OrderBy(c => c.CompNameAr)
                                .ToListAsync(),
                            "Id",
                            "CompNameAr",
                            companyId);
            }
            else
            {
                ViewBag.Companies = new SelectList(Enumerable.Empty<SelectListItem>());
            }


            // Get Car Type for dropdown
            ViewBag.DefTypeId = new SelectList(
                 await _context.Deffs
                    .Where(c => c.DeffType == 20)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName",
                DefTypeId);



            // Base query with includes
            var query = _context.DebitPayInfos
                 .FromSqlRaw($"SELECT * FROM DebitPayInfo where DebitInfoId IN(Select Id from DebitInfo where EmpId IN (Select ID From EmployeeInfo where CompanyId IN({companyIdsString})))")
                .Include(c => c.DebitInfo)
                .Include(c => c.DebitInfo!.Emp)
                .Include(c => c.DebitInfo!.DebitType)
                .Include(c => c.User)
                .Include(c => c.UserRecieved)
                .Where(m => m.DeleteFlag == 0 && m.DebitInfo!.Emp!.DeleteFlag == 0)
                .OrderBy(e => e.DebitPayNo);


            // Apply filters
            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpSearch))
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.FullNameAr!.Contains(EmpSearch));
            }

            if (DefTypeId.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.DebitTypeId == DefTypeId.Value);
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }
            //if (FromDateSearch == null && ToDateSearch == null)
            //{
            //    var today = DateOnly.FromDateTime(DateTime.Now);
            //    var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
            //    query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate >= sevenDaysAgo);
            //}
            // Store current search values for the view
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpFilter"] = EmpSearch;
            ViewData["DefTypeFilter"] = DefTypeId;
            ViewData["CompanyFilter"] = companyId;
            if (FromDateSearch == null)
            {
                ViewData["FromDateFilter"] = "";
            }
            else
            {
                ViewData["FromDateFilter"] = FromDateSearch.Value.ToString("yyyy-MM-dd");
            }
            if (ToDateSearch == null)
            {
                ViewData["ToDateFilter"] = "";
            }
            else
            {
                ViewData["ToDateFilter"] = ToDateSearch.Value.ToString("yyyy-MM-dd");
            }
            // Pagination

            decimal? totalBillPayed = (decimal?)query.Sum(item => item.DebitPayQty);

            ViewBag.TotalBillPayed = totalBillPayed;

            int pageSize = 50; // Set your page size
            return View(await PaginatedList<DebitPayInfo>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));


            //var rahalWebContext = _context.DebitPayInfos.Include(d => d.DebitInfo).Include(d => d.User).Include(d => d.UserRecieved).Include(d => d.Violation);
            //return View(await rahalWebContext.ToListAsync());
        }

        // GET: DebitPayInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debitPayInfo = await _context.DebitPayInfos
                .Include(d => d.DebitInfo)
                .Include(d => d.User)
                .Include(d => d.UserRecieved)
                .Include(d => d.ViolationInfo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debitPayInfo == null)
            {
                return NotFound();
            }

            return View(debitPayInfo);
        }

        // GET: DebitPayInfoes/Create
        public IActionResult Create()
        {
            ViewData["DebitInfoId"] = new SelectList(_context.DebitInfos, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id");
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id");
            ViewData["ViolationId"] = new SelectList(_context.Deffs, "Id", "Id");
            return View();
        }

        // POST: DebitPayInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DebitPayNo,DebitPayDate,DebitPayQty,DeleteFlag,ViolationId,UserId,UserRecievedId,UserRecievedDate,Hent,DeleteReson,DebitInfoId")] DebitPayInfo debitPayInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(debitPayInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DebitInfoId"] = new SelectList(_context.DebitInfos, "Id", "Id", debitPayInfo.DebitInfoId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserRecievedId);
            ViewData["ViolationId"] = new SelectList(_context.Deffs, "Id", "Id", debitPayInfo.ViolationId);
            return View(debitPayInfo);
        }

        // GET: DebitPayInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debitPayInfo = await _context.DebitPayInfos.FindAsync(id);
            if (debitPayInfo == null)
            {
                return NotFound();
            }
            ViewData["DebitInfoId"] = new SelectList(_context.DebitInfos, "Id", "Id", debitPayInfo.DebitInfoId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserRecievedId);
            ViewData["ViolationId"] = new SelectList(_context.Deffs, "Id", "Id", debitPayInfo.ViolationId);
            return View(debitPayInfo);
        }

        // POST: DebitPayInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DebitPayNo,DebitPayDate,DebitPayQty,DeleteFlag,ViolationId,UserId,UserRecievedId,UserRecievedDate,Hent,DeleteReson,DebitInfoId")] DebitPayInfo debitPayInfo)
        {
            if (id != debitPayInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(debitPayInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DebitPayInfoExists(debitPayInfo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DebitInfoId"] = new SelectList(_context.DebitInfos, "Id", "Id", debitPayInfo.DebitInfoId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserRecievedId);
            ViewData["ViolationId"] = new SelectList(_context.Deffs, "Id", "Id", debitPayInfo.ViolationId);
            return View(debitPayInfo);
        }

        // GET: DebitPayInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debitPayInfo = await _context.DebitPayInfos
                .Include(d => d.DebitInfo)
                .Include(d => d.User)
                .Include(d => d.UserRecieved)
                .Include(d => d.ViolationInfo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debitPayInfo == null)
            {
                return NotFound();
            }

            return View(debitPayInfo);
        }

        // POST: DebitPayInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var debitPayInfo = await _context.DebitPayInfos.FindAsync(id);
            if (debitPayInfo != null)
            {
                _context.DebitPayInfos.Remove(debitPayInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DebitPayInfoExists(int id)
        {
            return _context.DebitPayInfos.Any(e => e.Id == id);
        }

        public async Task<IActionResult> IndexReport(int? EmpCodeString, string? EmpSearch, int? DefTypeId, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
            // Get companies for dropdown
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            // Get companies for dropdown
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            //// Get the user's company data from TempData
            //// Get the user's company data from TempData
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            if (companyIds.Any())
            {
                ViewBag.Companies = new SelectList(
                            await _context.CompanyInfos
                                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                                .OrderBy(c => c.CompNameAr)
                                .ToListAsync(),
                            "Id",
                            "CompNameAr",
                            companyId);
            }
            else
            {
                ViewBag.Companies = new SelectList(Enumerable.Empty<SelectListItem>());
            }


            // Get Car Type for dropdown
            ViewBag.DefTypeId = new SelectList(
                 await _context.Deffs
                    .Where(c => c.DeffType == 20)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName",
                DefTypeId);



            // Base query with includes
            var query = _context.DebitPayInfos
                .FromSqlRaw($"select * from DebitPayInfo where DebitInfoId in (Select Id from DebitInfo where EmpId in ( Select Id from  EmployeeInfo where deleteFlag = 0 and  CompanyId  IN ({companyIdsString})))")
                .Include(c => c.DebitInfo)
                .Include(c => c.DebitInfo!.Emp)
                .Include(c => c.DebitInfo!.DebitType)
                .Include(c => c.User)
                .Include(c => c.UserRecieved)
                .Where(m => m.DeleteFlag == 0 && m.DebitInfo!.Emp!.DeleteFlag == 0)
                .OrderBy(e => e.DebitPayNo);


            // Apply filters
            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpSearch))
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.FullNameAr!.Contains(EmpSearch));
            }

            if (DefTypeId.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.DebitTypeId == DefTypeId.Value);
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }
            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate >= sevenDaysAgo);
            }
            // Store current search values for the view
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpFilter"] = EmpSearch;
            ViewData["DefTypeFilter"] = DefTypeId;
            ViewData["CompanyFilter"] = companyId;
            if (FromDateSearch == null)
            {
                ViewData["FromDateFilter"] = "";
            }
            else
            {
                ViewData["FromDateFilter"] = FromDateSearch.Value.ToString("yyyy-MM-dd");
            }
            if (ToDateSearch == null)
            {
                ViewData["ToDateFilter"] = "";
            }
            else
            {
                ViewData["ToDateFilter"] = ToDateSearch.Value.ToString("yyyy-MM-dd");
            }
            // Pagination

            decimal? totalBillPayed = (decimal?)query.Sum(item => item.DebitPayQty);

            ViewBag.TotalBillPayed = totalBillPayed;

            int pageSize = 10; // Set your page size
            return View(await PaginatedList<DebitPayInfo>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));


            //var rahalWebContext = _context.DebitPayInfos.Include(d => d.DebitInfo).Include(d => d.User).Include(d => d.UserRecieved).Include(d => d.Violation);
            //return View(await rahalWebContext.ToListAsync());
        }

        public IActionResult ExportToExcel(int? EmpCodeString, string? EmpSearch, int? DefTypeId, int? companyId, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
            TempData.Keep();
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData!.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            var query = _context.DebitPayInfos
                .FromSqlRaw($"select * from DebitPayInfo where DebitInfoId in (Select Id from DebitInfo where EmpId in ( Select Id from EmployeeInfo where deleteFlag = 0 and CompanyId IN ({companyIdsString})))")
                .Include(c => c.DebitInfo)
                .Include(c => c.DebitInfo!.Emp)
                .Include(c => c.DebitInfo!.DebitType)
                .Where(m => m.DeleteFlag == 0 && m.DebitInfo!.Emp!.DeleteFlag == 0);

            if (EmpCodeString.HasValue)
                query = query.Where(e => e.DebitInfo!.Emp!.EmpCode == EmpCodeString);
            if (!string.IsNullOrEmpty(EmpSearch))
                query = query.Where(e => e.DebitInfo!.Emp!.FullNameAr!.Contains(EmpSearch));
            if (DefTypeId.HasValue)
                query = query.Where(e => e.DebitInfo!.DebitTypeId == DefTypeId.Value);
            if (companyId.HasValue)
                query = query.Where(e => e.DebitInfo!.Emp!.CompanyId == companyId.Value);
            if (FromDateSearch.HasValue)
                query = query.Where(e => e.DebitPayDate >= DateOnly.FromDateTime(FromDateSearch.Value));
            if (ToDateSearch.HasValue)
                query = query.Where(e => e.DebitPayDate <= DateOnly.FromDateTime(ToDateSearch.Value));
            if (FromDateSearch == null && ToDateSearch == null)
            {
                var sevenDaysAgo = DateOnly.FromDateTime(DateTime.Now).AddDays(-7);
                query = query.Where(e => e.DebitPayDate >= sevenDaysAgo);
            }

            var data = query.OrderBy(e => e.DebitPayNo).ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("تحصيل الديون");
            ws.RightToLeft = true;

            ws.Cell(1, 1).Value = "تقرير تحصيل الديون";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(1, 1, 1, 7).Merge();

            ws.Cell(2, 1).Value = $"تاريخ التقرير: {DateTime.Today:yyyy/MM/dd}";
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(2, 1, 2, 7).Merge();

            string[] headers = { "#", "رقم فاتورة التحصيل", "التاريخ", "رقم الموظف", "الاسم", "نوع الدين", "المبلغ" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a1a2e");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            int row = 5;
            int num = 1;
            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = num++;
                ws.Cell(row, 2).Value = item.DebitPayNo;
                ws.Cell(row, 3).Value = item.DebitPayDate?.ToString("yyyy-MM-dd");
                ws.Cell(row, 4).Value = item.DebitInfo?.Emp?.EmpCode;
                ws.Cell(row, 5).Value = item.DebitInfo?.Emp?.FullNameAr;
                ws.Cell(row, 6).Value = item.DebitInfo?.DebitType?.DeffName;
                ws.Cell(row, 7).Value = item.DebitPayQty;

                for (int col = 1; col <= 7; col++)
                {
                    ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                if (row % 2 == 0)
                    ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");

                row++;
            }

            // Totals row
            ws.Cell(row, 1).Value = "الإجمالي";
            ws.Range(row, 1, row, 6).Merge();
            ws.Cell(row, 7).Value = data.Sum(d => d.DebitPayQty ?? 0);
            for (int col = 1; col <= 7; col++)
            {
                ws.Cell(row, col).Style.Font.Bold = true;
                ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromHtml("#f0f4f8");
                ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            ws.Column(7).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"تحصيل_الديون_{DateTime.Today:yyyyMMdd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

    }
}