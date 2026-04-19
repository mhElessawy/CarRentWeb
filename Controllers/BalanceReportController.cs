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

            // Bills query (daily rent = ContractType 0, monthly rent = ContractType 1)
            var billsQuery = _context.Bills
                .FromSqlRaw($"SELECT * FROM Bill WHERE EmployeeId IN (SELECT Id FROM EmployeeInfo WHERE CompanyId IN ({companyIdsString}))")
                .Include(b => b.Contract)
                .Where(b => b.DeleteFlag == 0);

            // DebitPayInfos query
            var debitsQuery = _context.DebitPayInfos
                .FromSqlRaw($"SELECT * FROM DebitPayInfo WHERE DebitInfoId IN (SELECT Id FROM DebitInfo WHERE EmpId IN (SELECT Id FROM EmployeeInfo WHERE CompanyId IN ({companyIdsString})))")
                .Where(d => d.DeleteFlag == 0);

            if (companyId.HasValue)
            {
                billsQuery = (IQueryable<Models.Bill>)billsQuery.Where(b => b.Employee!.CompanyId == companyId.Value);
                debitsQuery = (IQueryable<Models.DebitPayInfo>)debitsQuery.Where(d => d.DebitInfo!.Emp!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                var from = DateOnly.FromDateTime(FromDateSearch.Value);
                billsQuery = (IQueryable<Models.Bill>)billsQuery.Where(b => b.BillDate >= from);
                debitsQuery = (IQueryable<Models.DebitPayInfo>)debitsQuery.Where(d => d.DebitPayDate >= from);
            }

            if (ToDateSearch.HasValue)
            {
                var to = DateOnly.FromDateTime(ToDateSearch.Value);
                billsQuery = (IQueryable<Models.Bill>)billsQuery.Where(b => b.BillDate <= to);
                debitsQuery = (IQueryable<Models.DebitPayInfo>)debitsQuery.Where(d => d.DebitPayDate <= to);
            }

            var totalDailyRent = await billsQuery
                .Where(b => b.Contract!.ContractType == 0)
                .SumAsync(b => (decimal?)b.BillPayed) ?? 0;

            var totalMonthlyRent = await billsQuery
                .Where(b => b.Contract!.ContractType == 1)
                .SumAsync(b => (decimal?)b.BillPayed) ?? 0;

            var totalDebtsPaid = await debitsQuery
                .SumAsync(d => (decimal?)d.DebitPayQty) ?? 0;

            // Remaining debt from DebitInfo (not filtered by date — reflects current state)
            var debitInfoQuery = _context.DebitInfos
                .FromSqlRaw($"SELECT * FROM DebitInfo WHERE EmpId IN (SELECT Id FROM EmployeeInfo WHERE CompanyId IN ({companyIdsString}))")
                .Where(d => d.DeleteFlag == 0);

            if (companyId.HasValue)
                debitInfoQuery = (IQueryable<Models.DebitInfo>)debitInfoQuery.Where(d => d.Emp!.CompanyId == companyId.Value);

            var remainingDebt = await debitInfoQuery
                .SumAsync(d => (decimal?)d.DebitRemaining) ?? 0;

            var model = new BalanceReportViewModel
            {
                TotalDailyRentPaid = totalDailyRent,
                TotalMonthlyRentPaid = totalMonthlyRent,
                TotalDebtsPaid = totalDebtsPaid,
                RemainingDebt = remainingDebt
            };

            ViewData["CompanyFilter"] = companyId;
            ViewData["FromDateFilter"] = FromDateSearch?.ToString("yyyy-MM-dd") ?? "";
            ViewData["ToDateFilter"] = ToDateSearch?.ToString("yyyy-MM-dd") ?? "";

            return View(model);
        }
    }
}
