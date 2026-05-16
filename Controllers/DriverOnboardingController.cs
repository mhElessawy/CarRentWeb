using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRentWeb.Data;
using CarRentWeb.Models;

namespace CarRentWeb.Controllers
{
    public class DriverOnboardingController : Controller
    {
        private readonly CarRentWebContext _context;

        public DriverOnboardingController(CarRentWebContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep();

            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeInfo employeeInfo, string submitAction)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep();

            ModelState.Remove("FullNameAr");
            ModelState.Remove("FullNameEn");
            ModelState.Remove("DeleteFlag");
            ModelState.Remove("EmpDepMang");

            if (_context.EmployeeInfos.Any(a => a.EmpCode == employeeInfo.EmpCode && a.DeleteFlag == 0))
                ModelState.AddModelError("EmpCode", "كود السائق موجود مسبقاً");

            if (!string.IsNullOrEmpty(employeeInfo.CivilId) &&
                _context.EmployeeInfos.Any(a => a.CivilId == employeeInfo.CivilId && a.DeleteFlag == 0))
                ModelState.AddModelError("CivilId", "الرقم المدني موجود مسبقاً");

            if (ModelState.IsValid)
            {
                employeeInfo.FullNameAr = string.Join(" ",
                    new[] { employeeInfo.FirstNameAr, employeeInfo.SecondNameAr, employeeInfo.ThirdNameAr, employeeInfo.ForthNameAr, employeeInfo.LastNameAr }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

                employeeInfo.FullNameEn = string.Join(" ",
                    new[] { employeeInfo.FirstNameEn, employeeInfo.SecondNameEn, employeeInfo.ThirdNameEn, employeeInfo.ForthNameEn, employeeInfo.LastNameEn }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

                employeeInfo.DeleteFlag = 0;
                employeeInfo.RegDate = DateOnly.FromDateTime(DateTime.Today);
                employeeInfo.UserId = HttpContext.Session.GetInt32("UserId");

                _context.Add(employeeInfo);
                await _context.SaveChangesAsync();

                if (submitAction == "contract")
                    return RedirectToAction("IndexMonthly", "Contracts");

                return RedirectToAction("Index", "EmployeeInfoes");
            }

            LoadDropdowns();
            return View(employeeInfo);
        }

        private void LoadDropdowns()
        {
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString() ?? "";
            var companyIds = userCompanyData.Split(',')
                .Where(x => int.TryParse(x.Trim(), out _))
                .Select(x => int.Parse(x.Trim()))
                .ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            ViewData["CompanyId"] = new SelectList(
                _context.CompanyInfos.FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})"),
                "Id", "CompNameAr");
            ViewData["JobTitleId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 5), "Id", "DeffName");
            ViewData["NationalityId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 2), "Id", "DeffName");
            ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");
        }
    }
}
