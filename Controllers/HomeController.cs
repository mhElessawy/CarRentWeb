using CarRentWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentWeb.Data;
using CarRentWeb.Models;
using CarRentWeb.Models.MyModel;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace CarRentWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly CarRentWebContext _context;
        public HomeController(CarRentWebContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> IndexAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var sevenMonthAgo = today.AddMonths(-3); // Subtract 3 month
            var count = _context.EmployeeInfos.Count(a => a.ResEndDate <= sevenMonthAgo);
            ViewBag.ResignedEmployeeCount = count;
            count = _context.CarInfos.Count(a => a.CarEndLicense <= sevenMonthAgo);
            ViewBag.EndLicenseCarCount = count;

            count = _context.EmployeeInfos.Count(a => a.EndPerm <= sevenMonthAgo);
            ViewBag.EndPermEmpCount = count;

            count = _context.EmployeeInfos.Count(a => a.EndLicense <= sevenMonthAgo);
            ViewBag.EndLicenseEmpCount = count;

            // Contract notifications: contracts nearing EndDate (within 30 days)
            var thirtyDaysLater = today.AddDays(30);
            ViewBag.NearEndDateContractCount = _context.Contracts.Count(c =>
                c.DeleteFlag == 0 && c.Status == 0 &&
                c.EndDate != null &&
                c.EndDate >= today && c.EndDate <= thirtyDaysLater);

            // Contract notifications: contracts nearing DiscountDate (within 30 days)
            ViewBag.NearDiscountDateContractCount = _context.Contracts.Count(c =>
                c.DeleteFlag == 0 && c.Status == 0 &&
                c.DiscountDate != null &&
                c.DiscountDate >= today && c.DiscountDate <= thirtyDaysLater);

            // Cars with no active contract
            var activeCarsInContract = _context.Contracts
                .Where(c => c.DeleteFlag == 0 && c.Status == 0)
                .Select(c => c.CarId);
            ViewBag.CarsWithoutContractCount = _context.CarInfos
                .Count(c => c.DeleteFlag == 0 && !activeCarsInContract.Contains(c.Id));

            // Employees with no active contract
            var activeEmpsInContract = _context.Contracts
                .Where(c => c.DeleteFlag == 0 && c.Status == 0)
                .Select(c => c.EmployeeId);
            ViewBag.EmpsWithoutContractCount = _context.EmployeeInfos
                .Count(e => e.DeleteFlag == 0 && !activeEmpsInContract.Contains(e.Id));

            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values
            if (HttpContext.Session.GetInt32("UserId") == 0)
            {
                return RedirectToAction("Login", "PasswordDatums");
            }

            HttpContext.Session.SetObjectAsJson("CurrentUser", _context.PasswordData.Find(HttpContext.Session.GetInt32("UserId"))!);
            var user = HttpContext.Session.GetObjectFromJson<PasswordDatum>("CurrentUser");

            ViewBag.DeffInfo = await _context.DeffInformation.FirstOrDefaultAsync();

            return View();
        }
        public IActionResult PrintData(int id)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var threeMonthsAgo = today.AddMonths(-3);

            if (id == 1)
            {
                var data = _context.EmployeeInfos
                    .Where(a => a.ResEndDate <= threeMonthsAgo)
                    .ToList();

                ViewBag.Header = "إنتهاء الإقامات";
                ViewBag.DataType = "EmpRes";
                return View(data);
            }
            else if (id == 2)
            {
                var data = _context.CarInfos
                    .Where(a => a.CarEndLicense <= threeMonthsAgo)
                    .ToList();

                ViewBag.Header = "إنتهاء دفتر السيارة";
                ViewBag.DataType = "CarLicense";
                return View(data);
            }
            else if (id == 3)
            {
                var data = _context.EmployeeInfos
                    .Where(a => a.EndPerm <= threeMonthsAgo)
                    .ToList();

                ViewBag.Header = "إنتهاء إذن العمل";
                ViewBag.DataType = "EmpPerm";
                return View(data);
            }
            else if (id == 4)
            {
                var data = _context.EmployeeInfos
                    .Where(a => a.EndLicense <= threeMonthsAgo)
                    .ToList();

                ViewBag.Header = "إنتهاء رخصة سواقه";
                ViewBag.DataType = "EmpLicense";
                return View(data);
            }
            else if (id == 5)
            {
                var thirtyDaysLater = today.AddDays(30);
                var data = _context.Contracts
                    .Include(c => c.Employee)
                    .Include(c => c.Car)
                    .Where(c => c.DeleteFlag == 0 && c.Status == 0 &&
                                c.EndDate != null &&
                                c.EndDate >= today && c.EndDate <= thirtyDaysLater)
                    .OrderBy(c => c.EndDate)
                    .ToList();

                ViewBag.Header = "عقود تقترب من الانتهاء";
                ViewBag.DataType = "NearEndDate";
                return View(data);
            }
            else if (id == 6)
            {
                var thirtyDaysLater = today.AddDays(30);
                var data = _context.Contracts
                    .Include(c => c.Employee)
                    .Include(c => c.Car)
                    .Where(c => c.DeleteFlag == 0 && c.Status == 0 &&
                                c.DiscountDate != null &&
                                c.DiscountDate >= today && c.DiscountDate <= thirtyDaysLater)
                    .OrderBy(c => c.DiscountDate)
                    .ToList();

                ViewBag.Header = "عقود تقترب من موعد التخفيض";
                ViewBag.DataType = "NearDiscountDate";
                return View(data);
            }
            else if (id == 7)
            {
                var activeCarIds = _context.Contracts
                    .Where(c => c.DeleteFlag == 0 && c.Status == 0)
                    .Select(c => c.CarId);
                var data = _context.CarInfos
                    .Include(c => c.Company)
                    .Include(c => c.CarType)
                    .Where(c => c.DeleteFlag == 0 && !activeCarIds.Contains(c.Id))
                    .OrderBy(c => c.CarCode)
                    .ToList();

                ViewBag.Header = "سيارات بلا عقود";
                ViewBag.DataType = "CarsWithoutContract";
                return View(data);
            }
            else if (id == 8)
            {
                var activeEmpIds = _context.Contracts
                    .Where(c => c.DeleteFlag == 0 && c.Status == 0)
                    .Select(c => c.EmployeeId);
                var data = _context.EmployeeInfos
                    .Include(e => e.Company)
                    .Where(e => e.DeleteFlag == 0 && !activeEmpIds.Contains(e.Id))
                    .OrderBy(e => e.EmpCode)
                    .ToList();

                ViewBag.Header = "موظفين بلا عقود";
                ViewBag.DataType = "EmpsWithoutContract";
                return View(data);
            }

            return View();

        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> DeffTypeIndex(int? pageNumber)
        {
            int pageSize = 100;


            var items = _context.DeffTypes.AsQueryable(); // Assuming DbSet is named "DeffTypes"
                                                          //  var paginatedItems = await PaginatedList<DeffType>.CreateAsync(items, pageNumber, pageSize);
            return View(await PaginatedList<DeffType>.CreateAsync(items.AsNoTracking(), pageNumber ?? 1, pageSize));
            //  return View(paginatedItems);
        }


    }
}