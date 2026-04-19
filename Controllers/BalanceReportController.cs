using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRentWeb.Data;
using CarRentWeb.Models.MyModel;

namespace CarRentWeb.Controllers
{
    public class BalanceReportController : Controller
    {
        private readonly CarRentWebContext _context;

        public BalanceReportController(CarRentWebContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? companyId, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep();

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData!.Split(',')
                .Where(x => int.TryParse(x.Trim(), out _))
                .Select(x => int.Parse(x.Trim()))
                .ToList();
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

            // Employees in scope
            var empQuery = _context.EmployeeInfos
                .FromSqlRaw($"SELECT * FROM EmployeeInfo WHERE CompanyId IN ({companyIdsString}) AND DeleteFlag = 0")
                .Include(e => e.Company)
                .AsQueryable();

            if (companyId.HasValue)
                empQuery = empQuery.Where(e => e.CompanyId == companyId.Value);

            var employees = await empQuery
                .Select(e => new { e.Id, e.EmpCode, e.FullNameAr, e.MobiileNo, e.TelNo, CompanyName = e.Company!.CompNameAr })
                .ToListAsync();

            var empIds = employees.Select(e => e.Id).ToList();

            // OverdueRental: unpaid remaining rent from ContractDetails (Status == 0)
            var contractDetailsQuery = _context.ContractDetails
                .Where(cd => cd.Contract != null && empIds.Contains(cd.Contract.EmployeeId!.Value) && cd.Status == 0);

            if (FromDateSearch.HasValue)
                contractDetailsQuery = contractDetailsQuery.Where(cd => cd.DailyCreditDate >= DateOnly.FromDateTime(FromDateSearch.Value));
            if (ToDateSearch.HasValue)
                contractDetailsQuery = contractDetailsQuery.Where(cd => cd.DailyCreditDate <= DateOnly.FromDateTime(ToDateSearch.Value));

            var overdueByEmp = await contractDetailsQuery
                .GroupBy(cd => cd.Contract!.EmployeeId)
                .Select(g => new { EmpId = g.Key, Total = g.Sum(cd => (decimal?)cd.DailyCredit) ?? 0 })
                .ToListAsync();

            // RemainingDebt: unpaid debt from DebitInfo
            var debitQuery = _context.DebitInfos
                .Where(d => empIds.Contains(d.EmpId!.Value) && d.DeleteFlag == 0);

            var debtByEmp = await debitQuery
                .GroupBy(d => d.EmpId)
                .Select(g => new { EmpId = g.Key, Total = g.Sum(d => (decimal?)d.DebitRemaining) ?? 0 })
                .ToListAsync();

            // Build report items — only employees with overdue rent or remaining debt
            var items = employees
                .Select(e => new BalanceReportItemViewModel
                {
                    EmpCode = e.EmpCode ?? 0,
                    EmployeeName = e.FullNameAr,
                    MobileNo = e.MobiileNo ?? e.TelNo,
                    CompanyName = e.CompanyName,
                    OverdueRental = overdueByEmp.FirstOrDefault(o => o.EmpId == e.Id)?.Total ?? 0,
                    RemainingDebt = debtByEmp.FirstOrDefault(d => d.EmpId == e.Id)?.Total ?? 0
                })
                .Where(i => i.OverdueRental > 0 || i.RemainingDebt > 0)
                .OrderBy(i => i.CompanyName)
                .ThenBy(i => i.EmpCode)
                .ToList();

            var model = new BalanceReportViewModel { Items = items };

            ViewData["CompanyFilter"] = companyId;
            ViewData["FromDateFilter"] = FromDateSearch?.ToString("yyyy-MM-dd") ?? "";
            ViewData["ToDateFilter"] = ToDateSearch?.ToString("yyyy-MM-dd") ?? "";

            return View(model);
        }
    }
}
