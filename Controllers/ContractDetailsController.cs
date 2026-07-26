using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using ClosedXML.Excel;
using CarRentWeb.Models;
using CarRentWeb.Models.MyModel;

using System.Globalization;

using CarRentWeb.Data;

namespace CarRentWeb.Controllers
{
    public class ContractDetailsController : Controller
    {
        private readonly CarRentWebContext _context;
        private object _Context;

        public ContractDetailsController(CarRentWebContext context)
        {
            _context = context;
        }

        // GET: ContractDetails
        public async Task<IActionResult> Index(int? CarCodeString, int? EmpCodeString, string? EmpNameSearch, int? companyId, int? pageNumber, string? ContractNoString)
        {
            TempData.Keep();

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


            //   Base query with includes
            var baseQuery = _context.ContractDetails
                     .FromSqlRaw($"select * from ContractDetails where ContractId In (Select Id from Contract where DeleteFlag = 0 and status = 0 and  EmployeeId In ( Select Id From EmployeeInfo where CompanyId  IN ({companyIdsString})))")
                     .Include(c => c.Bill)
                     .Include(c => c.Contract)
                         .ThenInclude(c => c!.Employee)
                     .Include(c => c.Contract)
                         .ThenInclude(c => c!.Car)
                     .Where(a => a.DeleteFlag == 0
                         && (a.Status != 3 && a.Status != 4)
                         && a.Contract!.DeleteFlag == 0
                         && a.Contract!.Status == 0
                         && a.Contract!.ContractType == 1);


            //var query = baseQuery
            //    .Where(cd => cd.DailyCreditDate > _context.ContractDetails
            //            .Where(last => last.ContractId == cd.ContractId && last.Status == 3)
            //            .OrderByDescending(last => last.Id)
            //            .Select(last => last.DailyCreditDate)
            //            .FirstOrDefault());

            var query = baseQuery
                .Where(cd =>
                    // إذا كان فيه Status = 3
                    _context.ContractDetails
                        .Where(last => last.ContractId == cd.ContractId && last.Status == 3)
                        .OrderByDescending(last => last.Id)
                        .Select(last => last.DailyCreditDate)
                        .FirstOrDefault() != null
                    ?
                        // نجيب السجلات اللي بعد آخر Status = 3
                        cd.DailyCreditDate > _context.ContractDetails
                            .Where(last => last.ContractId == cd.ContractId && last.Status == 3)
                            .OrderByDescending(last => last.Id)
                            .Select(last => last.DailyCreditDate)
                            .FirstOrDefault()
                    :
                        // إذا مكنش فيه Status = 3، نجيب أول سجل فقط
                        cd.Id == _context.ContractDetails
                            .Where(first => first.ContractId == cd.ContractId)
                            .OrderBy(first => first.Id)
                            .Select(first => first.Id)
                            .FirstOrDefault()
                );


            // Apply filters
            if (!string.IsNullOrEmpty(ContractNoString))
            {
                query = query.Where(e => e.Contract!.ContractNo!.Contains(ContractNoString));
            }

            if (CarCodeString.HasValue)
            {
                query = query.Where(e => e.Contract!.Car!.CarCode == CarCodeString);
            }

            if (EmpCodeString.HasValue)
            {
                query = query.Where(e => e.Contract!.Employee!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = query.Where(e => e.Contract!.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }

            if (companyId.HasValue)
            {
                query = query.Where(e => e.Contract!.Employee!.CompanyId == companyId.Value);
            }

            // Get distinct employees by grouping
            var distinctEmployees = query
                .GroupBy(c => c.Contract!.Employee!.Id)
                .Select(g => g.First());

            //var debitByEmployee = _context.DebitInfos
            //    .GroupBy(e => e.EmpId)
            //    .Select(g => new
            //    {
            //        EmployeeId = g.Key,
            //        TotalDebitRemaining = g.Sum(x => x.DebitRemaining)
            //    });

            //var result = distinctEmployees
            //    .GroupJoin(debitByEmployee,
            //        employee => employee!.Contract!.Employee!.Id,
            //        debit => debit.EmployeeId,
            //        (employee, debitGroup) => new
            //        {
            //            Employee = employee.Contract!.Employee,
            //            Contract = employee.Contract,
            //            TotalDebitRemaining = debitGroup.Any() ? debitGroup.First().TotalDebitRemaining : 0
            //        })
            //    .ToList();


            // Store current search values for the view
            ViewData["ContractNoFilter"] = ContractNoString;
            ViewData["CarCodeFilter"] = CarCodeString;
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpNameFilter"] = EmpNameSearch;
            ViewData["CompanyFilter"] = companyId;

            // Pagination
            //int pageSize = 50;
            // return View(await PaginatedList<ContractDetail>.CreateAsync(distinctQuery.AsNoTracking(), pageNumber ?? 1, pageSize));
            return View(distinctEmployees);
        }

        // GET: ContractDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contractDetail = await _context.ContractDetails
                .Include(c => c.Bill)
                .Include(c => c.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contractDetail == null)
            {
                return NotFound();
            }

            return View(contractDetail);
        }

        // GET: ContractDetails/Create
        public IActionResult Create()
        {
            ViewData["BillId"] = new SelectList(_context.Bills, "Id", "Id");
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "Id");
            return View();
        }

        // POST: ContractDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractDetail contractDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contractDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BillId"] = new SelectList(_context.Bills, "Id", "Id", contractDetail.BillId);
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "Id", contractDetail.ContractId);
            return View(contractDetail);
        }

        // GET: ContractDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contractDetail = await _context.ContractDetails.FindAsync(id);
            if (contractDetail == null)
            {
                return NotFound();
            }
            ViewData["BillId"] = new SelectList(_context.Bills, "Id", "Id", contractDetail.BillId);
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "Id", contractDetail.ContractId);
            return View(contractDetail);
        }

        // POST: ContractDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractDetail contractDetail)
        {
            if (id != contractDetail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contractDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractDetailExists(contractDetail.Id))
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
            ViewData["BillId"] = new SelectList(_context.Bills, "Id", "Id", contractDetail.BillId);
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "Id", contractDetail.ContractId);
            return View(contractDetail);
        }

        // GET: ContractDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contractDetail = await _context.ContractDetails
                .Include(c => c.Bill)
                .Include(c => c.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contractDetail == null)
            {
                return NotFound();
            }

            return View(contractDetail);
        }

        // POST: ContractDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contractDetail = await _context.ContractDetails.FindAsync(id);
            if (contractDetail != null)
            {
                _context.ContractDetails.Remove(contractDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContractDetailExists(int id)
        {
            return _context.ContractDetails.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contractDetail = await _context.ContractDetails
                .Include(c => c.Bill)
                .Include(c => c.Contract)
                .Include(c => c.Contract!.Employee)
                .Include(c => c.Contract!.Car)
                .FirstOrDefaultAsync(m => m.Id == id);
            //  .Where(a => a.DailyCredit != 0 || a.CarCredit != 0)
            if (contractDetail == null)
            {
                return NotFound();
            }
            double debitPayLateDay = _context.DeffInformation
                                    .FirstOrDefault()?
                                    .DebitPayLateDay ?? 0;
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly? creditDate = (DateOnly?)contractDetail.DailyCreditDate;



            int daysDifference = currentDate.DayNumber - creditDate!.Value.DayNumber;
            ViewBag.LatePay = daysDifference > 0 ? daysDifference * debitPayLateDay : 0;

            ViewBag.LatePayId = _context.DeffInformation
                                    .FirstOrDefault()?
                                    .DebitPayLatId ?? 0;


            // Only calculate late pay if daysDifference is positive (payment is late)


            return View(contractDetail);
        }
        [HttpPost]
        public async Task<IActionResult> Pay(int? id, ContractDetail contractDetails, double latePay, int NoOfMonth, int latePayId)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {


                    var existingDetail = await _context.ContractDetails
                        .Include(c => c.Contract)
                        .Where(c => c.Contract!.Id == contractDetails.ContractId && (c.Status == 0 || c.Status == 2))
                        .Where(c =>
                            // إذا كان فيه Status = 3
                            _context.ContractDetails
                                .Where(last => last.ContractId == contractDetails.ContractId && last.Status == 3)
                                .OrderByDescending(last => last.Id)
                                .Select(last => last.DailyCreditDate)
                                .FirstOrDefault() != null
                            ?
                                // نجيب السجلات اللي بعد آخر Status = 3
                                c.DailyCreditDate > _context.ContractDetails
                                    .Where(last => last.ContractId == contractDetails.ContractId && last.Status == 3)
                                    .OrderByDescending(last => last.Id)
                                    .Select(last => last.DailyCreditDate)
                                    .FirstOrDefault()
                            :
                                // إذا مكنش فيه Status = 3، نجيب أول سجل فقط
                                c.DailyCreditDate >= _context.ContractDetails
                                    .Where(first => first.ContractId == contractDetails.ContractId && (first.Status == 0 || first.Status == 2))
                                    .OrderBy(first => first.DailyCreditDate)
                                    .Select(first => first.DailyCreditDate)
                                    .FirstOrDefault()
                        )
                        .OrderBy(c => c.Id)
                        .Take(NoOfMonth)
                        .ToListAsync();

                    DateOnly toDate = default;
                    DateOnly fromdate = default;
                    decimal? totalDailyCreditAndCarCredit = 0;
                    int EmpId = 0;
                    for (int i = 0; i < existingDetail.Count; i++)
                    {
                        if (i == 0)
                        {
                            //  fromdate = (DateOnly)existingDetail[i].DailyCreditDate!.Value.AddMonths(-1).AddDays(1);
                            fromdate = new DateOnly(existingDetail[i].DailyCreditDate!.Value.Year,
                                                       existingDetail[i].DailyCreditDate!.Value.Month,
                                                       1);
                            EmpId = (int)existingDetail[i].Contract!.EmployeeId!;
                        }
                        if (i == existingDetail.Count - 1)
                        {
                            toDate = (DateOnly)existingDetail[i].DailyCreditDate!;
                        }
                        totalDailyCreditAndCarCredit += (decimal?)(existingDetail[i].DailyCredit + (decimal?)existingDetail[i].CarCredit);
                    }
                    if (existingDetail == null)
                    {
                        return NotFound();
                    }
                    // add bill

                    int maxBillNo = await _context.Bills.MaxAsync(b => (int)b.BillNo!);

                    ViewBag.MaxmaxBillNo = maxBillNo + 1;
                    string billhent = contractDetails.CarCredit > 0 ? "إيجار + قسط" : "إيجار";
                    var bill = new Bill
                    {
                        BillNo = ViewBag.MaxmaxBillNo,
                        ContractId = contractDetails.ContractId,
                        UserId = HttpContext.Session.GetInt32("UserId"),
                        UserRecievedId = HttpContext.Session.GetInt32("UserId"),
                        BillPayed = totalDailyCreditAndCarCredit,
                        BillDate = DateOnly.FromDateTime(DateTime.Now),
                        BillTime = TimeOnly.FromDateTime(DateTime.Now),
                        FromDate = fromdate,
                        ToDate = toDate,
                        NoOfDays = toDate.DayNumber - fromdate.DayNumber,
                        EmployeeId = EmpId,
                        DeleteFlag = 0,
                        BillHent = billhent,
                        BankIntNo = 568,
                    };

                    _context.Add(bill);
                    await _context.SaveChangesAsync();

                    int billId = _context.Bills.Max(a => Convert.ToInt32(a.Id));

                    for (int i = 0; i < existingDetail.Count; i++)
                    {
                        if (existingDetail[i].Status == 0)
                        {
                            existingDetail[i].Status = 3;
                        }


                        existingDetail[i].BillId = billId;
                        existingDetail[i].PayedDate = DateOnly.FromDateTime(DateTime.Now);
                        _context.Update(existingDetail[i]);
                    }

                    await _context.SaveChangesAsync();
                    // save debitlate and pay it 
                    if (latePay != 0)
                    {
                        int maxDebitNo = await _context.DebitInfos.MaxAsync(b => (int)b.DebitNo!);

                        ViewBag.MaxDebitNo = maxDebitNo + 1;
                        string DebitDescription = "غرامة تأخير تحصيل";
                        var debitIfo = new DebitInfo
                        {
                            DebitNo = ViewBag.MaxDebitNo,
                            EmpId = EmpId,
                            UserId = HttpContext.Session.GetInt32("UserId"),
                            DebitTypeId = latePayId,
                            DebitDate = DateOnly.FromDateTime(DateTime.Now),
                            DebitDescrp = DebitDescription,
                            DeleteFlag = 0,
                            ViolationId = 0,
                            DeleteReson = "",
                            DebitPayed = (decimal?)latePay,
                            DebitQty = (decimal?)latePay,
                            DebitRemaining = 0,
                        };
                        _context.Add(debitIfo);
                        await _context.SaveChangesAsync();

                        // save payed for DebitPayed

                        int MaxDebitInfoId = await _context.DebitInfos.MaxAsync(b => (int)b.Id!);

                        int maxDebitPayNo = await _context.DebitPayInfos.MaxAsync(b => (int)b.DebitPayNo!);
                        ViewBag.MaxDebitPayNo = maxDebitPayNo + 1;

                        var debitPayInfo = new DebitPayInfo
                        {
                            DebitPayNo = ViewBag.MaxDebitPayNo,
                            DebitPayDate = DateOnly.FromDateTime(DateTime.Now),
                            DebitPayQty = (decimal?)latePay,
                            DeleteFlag = 0,
                            ViolationId = 0,
                            UserId = HttpContext.Session.GetInt32("UserId"),
                            UserRecievedId = HttpContext.Session.GetInt32("UserId"),
                            DebitInfoId = MaxDebitInfoId,
                        };

                        _context.DebitPayInfos.Add(debitPayInfo);
                        await _context.SaveChangesAsync();

                    }
                    return RedirectToAction("PayPrint", "ContractDetails", new { Id = billId });

                    // return RedirectToAction(nameof(Index));
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractDetailExists(contractDetails.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return NotFound();
        }
        public IActionResult PayPrint(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var printBill = _context.Bills
                .Include(c => c.Employee)
                .Include(c => c.User)
                .Include(c => c.Contract)
                .Where(c => c.Id == Id)
                .FirstOrDefault();

            if (printBill == null)
            {
                return NotFound();
            }

            var latePay = _context.DebitInfos
                .Where(d => d.EmpId == printBill.EmployeeId &&
                           d.DebitDate == printBill.BillDate &&
                           d.DebitTypeId == 452)
                .Select(d => d.DebitPayed)
                .FirstOrDefault();
            ViewBag.LatePay = latePay ?? 0;
            ViewBag.NoOfCredit = _context.ContractDetails.Count(a => a.CarCredit != 0 && a.Status != 3 && a.ContractId == printBill.ContractId);
            return View(printBill);
        }
        [HttpGet]
        public IActionResult IndexMonthlyDetails(int? id)
        {
            TempData.Keep();
            var query = _context.ContractDetails.Include(c => c.Bill).Include(c => c.Contract)
               .Include(c => c.Contract!.Employee)
               .Include(c => c.Contract!.Car)
               .Where(a => a.DeleteFlag == 0 && a.ContractId == id && (a.DailyCredit != 0 || a.CarCredit != 0) && a.Status != 3);
            return View(query);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentDetails(int contractId, int months)
        {
            try
            {
                //var unpaidDetails = await _context.ContractDetails
                //     .Where(cd => cd.ContractId == contractId && cd.Status != 3 && cd.Status != 4)
                //     .Where(cd => cd.DailyCreditDate >
                //         _context.ContractDetails
                //             .Where(last => last.ContractId == cd.ContractId && last.Status == 3)
                //             .OrderByDescending(last => last.Id)
                //             .Select(last => last.DailyCreditDate)
                //             .FirstOrDefault())
                //     .OrderBy(cd => cd.DailyCreditDate)
                //     .Take(months)
                //     .ToListAsync();

                var unpaidDetails = await _context.ContractDetails
                        .Where(cd => cd.ContractId == contractId && cd.Status != 3 && cd.Status != 4)
                        .Where(cd =>
                            // إذا كان فيه Status = 3
                            _context.ContractDetails
                                .Where(last => last.ContractId == cd.ContractId && last.Status == 3)
                                .OrderByDescending(last => last.Id)
                                .Select(last => last.DailyCreditDate)
                                .FirstOrDefault() != null
                            ?
                                // نجيب السجلات اللي بعد آخر Status = 3
                                cd.DailyCreditDate > _context.ContractDetails
                                    .Where(last => last.ContractId == cd.ContractId && last.Status == 3)
                                    .OrderByDescending(last => last.Id)
                                    .Select(last => last.DailyCreditDate)
                                    .FirstOrDefault()
                            :
                                // إذا مكنش فيه Status = 3، نجيب أول سجل فقط
                                cd.DailyCreditDate >= _context.ContractDetails
                                    .Where(first => first.ContractId == cd.ContractId)
                                    .OrderBy(first => first.DailyCreditDate)
                                    .Select(first => first.DailyCreditDate)
                                    .FirstOrDefault()
                        )
                        .OrderBy(cd => cd.DailyCreditDate)
                        .Take(months)
                        .ToListAsync();

                if (!unpaidDetails.Any())
                {
                    return Json(new { success = false, message = "لا توجد أقساط غير مدفوعة" });
                }

                decimal? totalDailyCredit = (decimal?)unpaidDetails.Sum(cd => cd.DailyCredit);
                decimal? totalCarCredit = (decimal?)unpaidDetails.Sum(cd => cd.CarCredit);

                // Get the maximum date and format it properly
                DateOnly? lastDate = (DateOnly?)unpaidDetails.Max(cd => cd.DailyCreditDate);
                string? formattedDate = lastDate?.ToString("yyyy/MM/dd");

                return Json(new
                {
                    success = true,
                    totalDailyCredit = totalDailyCredit,
                    totalCarCredit = totalCarCredit,
                    lastDate = formattedDate // Use the formatted date
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public async Task<IActionResult> IndexReportAudit(int? selectMonth, int? selectYear, int? KindOfPay, int[] companyId)
        {
            TempData.Keep();

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

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

            ViewBag.SelectMonth = new SelectList(
                   Enumerable.Range(1, 12).Select(x => new
                   {
                       Value = x,
                       Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x)
                   }),
                   "Value",
                   "Text",
                   selectMonth);

            int currentYear = DateTime.Now.Year;
            ViewBag.SelectYear = new SelectList(
                Enumerable.Range(currentYear - 3, 4) // Last 3 years + current year = 4 years total
                    .OrderByDescending(y => y)       // Show in descending order (newest first)
                    .Select(y => new
                    {
                        Value = y,
                        Text = y.ToString()
                    }),
                "Value",
                "Text",
                selectYear);

            ViewBag.KindOfPay = new SelectList(
                    new List<SelectListItem>
                    {
                        new SelectListItem { Value = "1", Text = "مدفوع" },   // Paid
                        new SelectListItem { Value = "0", Text = "غير مدفوع" }  // Unpaid
                    },
                    "Value",
                    "Text",
                    KindOfPay);

            if (KindOfPay == null)
            {
                ModelState.AddModelError("KindOfPay", "يجب إختيار حالة الدفع");
            }
            else if (selectMonth == null)
            {
                ModelState.AddModelError("SelectMonth", "يجب إختيار الشهر");
            }
            else if (selectYear == null)
            {
                ModelState.AddModelError("SelectYear", "يجب إختيار السنه");
            }

            IQueryable<ContractDetail> query = _context.ContractDetails
                 .Where(a => false); // Start with empty result

            // Only filter if all required parameters are present
            if (KindOfPay.HasValue && selectMonth.HasValue && selectYear.HasValue)
            {
                query = _context.ContractDetails
                    .FromSqlRaw($"select * from ContractDetails where ContractId In (Select Id from Contract where DeleteFlag = 0 and status = 0 and  EmployeeId In ( Select Id From EmployeeInfo where CompanyId  IN ({companyIdsString})))")
                    .Include(c => c.Bill)
                    .Include(c => c.Contract)
                        .ThenInclude(c => c!.Employee)
                    .Include(c => c.Contract)
                        .ThenInclude(c => c!.Car)
                    .Where(a => a.DeleteFlag == 0 && (a.DailyCredit != 0 || a.CarCredit != 0));

                if (KindOfPay == 0)
                {
                    query = query.Where(a => a.Status == 0);
                }
                else
                {
                    query = query.Where(a => a.Status == 3);
                }

                query = query.Where(a =>
                    a.DailyCreditDate!.Value.Month == selectMonth &&
                    a.DailyCreditDate!.Value.Year == selectYear);

                if (companyId == null || companyId.Length == 0)
                {

                }
                else
                {

                    query = query.Where(e => e.Contract!.Employee!.CompanyId == companyId[0]);   //== companyId.Value
                }


                if (companyId != null && companyId.Length > 0)
                {
                    var selectedCompanyIds = companyId.ToList();
                    query = query.Where(e => selectedCompanyIds.Contains((int)e.Contract!.Employee!.CompanyId));
                }

            }

            var result = await query
                            .Where(c => c.Contract != null &&
                                        c.Contract.Employee != null &&
                                        c.Contract.Employee.EmpCode != null) // Ensure no nulls
                            .GroupBy(c => c.Contract!.Employee!.Id) // Safe after filtering
                            .Select(g => new ContractDetailsSumation
                            {
                                EmployeeId = g.Key,
                                EmpCode = (int)g.First().Contract!.Employee!.EmpCode!, // Now safe
                                MobileNo = g.First().Contract!.Employee!.MobiileNo ?? "N/A", // Fallback if null
                                EmployeeName = g.First().Contract!.Employee!.FullNameAr ?? "Unknown",
                                TotalDailyCredit = (decimal)g.Sum(c => c.DailyCredit),
                                TotalCarCredit = (decimal)g.Sum(c => c.CarCredit)
                            })
                            .OrderBy(x => x.EmpCode)
                            .ToListAsync();


            return View(result);


        }
        [HttpGet]
        public IActionResult IndexMonthlyDetailsPayed(int? id)
        {
            TempData.Keep();
            var query = _context.ContractDetails.Include(c => c.Bill).Include(c => c.Contract)
               .Include(c => c.Contract!.Employee)
               .Include(c => c.Contract!.Car)
               .Where(a => a.DeleteFlag == 0 && a.ContractId == id && (a.DailyCredit != 0 || a.CarCredit != 0) && a.Status == 3);
            return View(query);
        }

        public async Task<IActionResult> BalanceReport(int? EmpCodeString, string? EmpSearch, int? companyId)
        {
            TempData.Keep();
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            var companiesList = companyIds.Any()
                ? await _context.CompanyInfos
                    .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync()
                : new List<CompanyInfo>();

            ViewBag.Companies = new SelectList(companiesList, "Id", "CompNameAr", companyId);

            var today = DateOnly.FromDateTime(DateTime.Today);

            // Overdue rentals: ContractDetails where Status=0 and DailyCreditDate < today
            var rentalQuery = _context.ContractDetails
                .FromSqlRaw($"select * from ContractDetails where ContractId In (Select Id from Contract where DeleteFlag = 0 and status = 0 and EmployeeId In (Select Id From EmployeeInfo where DeleteFlag = 0 and CompanyId IN ({companyIdsString})))")
                .Include(c => c.Contract)
                    .ThenInclude(c => c!.Employee)
                        .ThenInclude(e => e!.Company)
                .Where(a => a.DeleteFlag == 0
                         && a.Status == 0
                         && a.DailyCreditDate < today
                         && (a.DailyCredit != 0 || a.CarCredit != 0)
                         && a.Contract!.Employee!.DeleteFlag == 0);

            if (companyId.HasValue)
                rentalQuery = rentalQuery.Where(e => e.Contract!.Employee!.CompanyId == companyId.Value);
            if (EmpCodeString.HasValue)
                rentalQuery = rentalQuery.Where(e => e.Contract!.Employee!.EmpCode == EmpCodeString.Value);
            if (!string.IsNullOrEmpty(EmpSearch))
                rentalQuery = rentalQuery.Where(e => e.Contract!.Employee!.FullNameAr!.Contains(EmpSearch));

            var rentalData = await rentalQuery
                .Where(c => c.Contract != null && c.Contract.Employee != null && c.Contract.Employee.EmpCode != null)
                .GroupBy(c => c.Contract!.Employee!.Id)
                .Select(g => new
                {
                    EmpId = g.Key,
                    EmpCode = (int)g.First().Contract!.Employee!.EmpCode!,
                    EmployeeName = g.First().Contract!.Employee!.FullNameAr ?? "",
                    MobileNo = g.First().Contract!.Employee!.MobiileNo ?? "",
                    CompanyName = g.First().Contract!.Employee!.Company!.CompNameAr ?? "",
                    OverdueRental = g.Sum(c => (c.DailyCredit ?? 0) + (c.CarCredit ?? 0))
                })
                .ToListAsync();

            // Overdue debts: DebitInfo where DebitRemaining > 0
            var debitQuery = _context.DebitInfos
                .FromSqlRaw($"select * from DebitInfo where EmpId In (Select Id From EmployeeInfo where DeleteFlag = 0 and CompanyId IN ({companyIdsString}))")
                .Include(d => d.Emp)
                    .ThenInclude(e => e!.Company)
                .Where(d => d.DeleteFlag == 0 && d.DebitRemaining > 0 && d.Emp!.DeleteFlag == 0);

            if (companyId.HasValue)
                debitQuery = debitQuery.Where(d => d.Emp!.CompanyId == companyId.Value);
            if (EmpCodeString.HasValue)
                debitQuery = debitQuery.Where(d => d.Emp!.EmpCode == EmpCodeString.Value);
            if (!string.IsNullOrEmpty(EmpSearch))
                debitQuery = debitQuery.Where(d => d.Emp!.FullNameAr!.Contains(EmpSearch));

            var debitData = await debitQuery
                .Where(d => d.Emp != null && d.Emp.EmpCode != null)
                .GroupBy(d => d.Emp!.Id)
                .Select(g => new
                {
                    EmpId = g.Key,
                    EmpCode = (int)g.First().Emp!.EmpCode!,
                    EmployeeName = g.First().Emp!.FullNameAr ?? "",
                    MobileNo = g.First().Emp!.MobiileNo ?? "",
                    CompanyName = g.First().Emp!.Company!.CompNameAr ?? "",
                    RemainingDebt = g.Sum(d => d.DebitRemaining ?? 0)
                })
                .ToListAsync();

            // Merge both lists by employee Id
            var allEmpIds = rentalData.Select(r => r.EmpId)
                .Union(debitData.Select(d => d.EmpId))
                .Distinct();

            var items = allEmpIds.Select(empId =>
            {
                var r = rentalData.FirstOrDefault(x => x.EmpId == empId);
                var d = debitData.FirstOrDefault(x => x.EmpId == empId);
                return new BalanceReportItemViewModel
                {
                    EmpCode = r?.EmpCode ?? d?.EmpCode ?? 0,
                    EmployeeName = r?.EmployeeName ?? d?.EmployeeName ?? "",
                    MobileNo = r?.MobileNo ?? d?.MobileNo ?? "",
                    CompanyName = r?.CompanyName ?? d?.CompanyName ?? "",
                    OverdueRental = r?.OverdueRental ?? 0,
                    RemainingDebt = d?.RemainingDebt ?? 0
                };
            })
            .OrderBy(x => x.EmpCode)
            .ToList();

            var viewModel = new BalanceReportViewModel { Items = items };

            return View(viewModel);
        }
        public async Task<IActionResult> BalanceReportExcel(int? EmpCodeString, string? EmpSearch, int? companyId)
        {
            TempData.Keep();
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData!.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            var today = DateOnly.FromDateTime(DateTime.Today);

            var rentalQuery = _context.ContractDetails
                .FromSqlRaw($"select * from ContractDetails where ContractId In (Select Id from Contract where DeleteFlag = 0 and status = 0 and EmployeeId In (Select Id From EmployeeInfo where DeleteFlag = 0 and CompanyId IN ({companyIdsString})))")
                .Include(c => c.Contract)
                    .ThenInclude(c => c!.Employee)
                        .ThenInclude(e => e!.Company)
                .Where(a => a.DeleteFlag == 0
                         && a.Status == 0
                         && a.DailyCreditDate < today
                         && (a.DailyCredit != 0 || a.CarCredit != 0)
                         && a.Contract!.Employee!.DeleteFlag == 0);

            if (companyId.HasValue)
                rentalQuery = rentalQuery.Where(e => e.Contract!.Employee!.CompanyId == companyId.Value);
            if (EmpCodeString.HasValue)
                rentalQuery = rentalQuery.Where(e => e.Contract!.Employee!.EmpCode == EmpCodeString.Value);
            if (!string.IsNullOrEmpty(EmpSearch))
                rentalQuery = rentalQuery.Where(e => e.Contract!.Employee!.FullNameAr!.Contains(EmpSearch));

            var rentalData = await rentalQuery
                .Where(c => c.Contract != null && c.Contract.Employee != null && c.Contract.Employee.EmpCode != null)
                .GroupBy(c => c.Contract!.Employee!.Id)
                .Select(g => new
                {
                    EmpId = g.Key,
                    EmpCode = (int)g.First().Contract!.Employee!.EmpCode!,
                    EmployeeName = g.First().Contract!.Employee!.FullNameAr ?? "",
                    MobileNo = g.First().Contract!.Employee!.MobiileNo ?? "",
                    CompanyName = g.First().Contract!.Employee!.Company!.CompNameAr ?? "",
                    OverdueRental = g.Sum(c => (c.DailyCredit ?? 0) + (c.CarCredit ?? 0))
                })
                .ToListAsync();

            var debitQuery = _context.DebitInfos
                .FromSqlRaw($"select * from DebitInfo where EmpId In (Select Id From EmployeeInfo where DeleteFlag = 0 and CompanyId IN ({companyIdsString}))")
                .Include(d => d.Emp)
                    .ThenInclude(e => e!.Company)
                .Where(d => d.DeleteFlag == 0 && d.DebitRemaining > 0 && d.Emp!.DeleteFlag == 0);

            if (companyId.HasValue)
                debitQuery = debitQuery.Where(d => d.Emp!.CompanyId == companyId.Value);
            if (EmpCodeString.HasValue)
                debitQuery = debitQuery.Where(d => d.Emp!.EmpCode == EmpCodeString.Value);
            if (!string.IsNullOrEmpty(EmpSearch))
                debitQuery = debitQuery.Where(d => d.Emp!.FullNameAr!.Contains(EmpSearch));

            var debitData = await debitQuery
                .Where(d => d.Emp != null && d.Emp.EmpCode != null)
                .GroupBy(d => d.Emp!.Id)
                .Select(g => new
                {
                    EmpId = g.Key,
                    EmpCode = (int)g.First().Emp!.EmpCode!,
                    EmployeeName = g.First().Emp!.FullNameAr ?? "",
                    MobileNo = g.First().Emp!.MobiileNo ?? "",
                    CompanyName = g.First().Emp!.Company!.CompNameAr ?? "",
                    RemainingDebt = g.Sum(d => d.DebitRemaining ?? 0)
                })
                .ToListAsync();

            var allEmpIds = rentalData.Select(r => r.EmpId)
                .Union(debitData.Select(d => d.EmpId))
                .Distinct();

            var items = allEmpIds.Select(empId =>
            {
                var r = rentalData.FirstOrDefault(x => x.EmpId == empId);
                var d = debitData.FirstOrDefault(x => x.EmpId == empId);
                return new BalanceReportItemViewModel
                {
                    EmpCode = r?.EmpCode ?? d?.EmpCode ?? 0,
                    EmployeeName = r?.EmployeeName ?? d?.EmployeeName ?? "",
                    MobileNo = r?.MobileNo ?? d?.MobileNo ?? "",
                    CompanyName = r?.CompanyName ?? d?.CompanyName ?? "",
                    OverdueRental = r?.OverdueRental ?? 0,
                    RemainingDebt = d?.RemainingDebt ?? 0
                };
            })
            .OrderBy(x => x.EmpCode)
            .ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("تقرير الرصيد");

            ws.RightToLeft = true;

            // Title row
            ws.Cell(1, 1).Value = "تقرير الرصيد الحالي";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(1, 1, 1, 8).Merge();

            ws.Cell(2, 1).Value = $"تاريخ التقرير: {DateTime.Today:yyyy/MM/dd}";
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(2, 1, 2, 8).Merge();

            // Header row
            int headerRow = 4;
            string[] headers = { "#", "كود الموظف", "اسم الموظف", "رقم الهاتف", "الشركة", "المبالغ المتأخرة", "الديون المتبقية", "الإجمالي" };
            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cell(headerRow, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a1a2e");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Data rows
            int row = headerRow + 1;
            int num = 1;
            foreach (var item in items)
            {
                ws.Cell(row, 1).Value = num++;
                ws.Cell(row, 2).Value = item.EmpCode;
                ws.Cell(row, 3).Value = item.EmployeeName;
                ws.Cell(row, 4).Value = item.MobileNo;
                ws.Cell(row, 5).Value = item.CompanyName;
                ws.Cell(row, 6).Value = item.OverdueRental;
                ws.Cell(row, 7).Value = item.RemainingDebt;
                ws.Cell(row, 8).Value = item.OverdueRental + item.RemainingDebt;

                for (int col = 1; col <= 8; col++)
                {
                    ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                ws.Cell(row, 6).Style.Font.FontColor = XLColor.FromHtml("#c0392b");
                ws.Cell(row, 7).Style.Font.FontColor = XLColor.FromHtml("#d35400");
                ws.Cell(row, 8).Style.Font.Bold = true;

                if (row % 2 == 0)
                    ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");

                row++;
            }

            // Totals row
            ws.Cell(row, 1).Value = "الإجمالي";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 6).Value = items.Sum(x => x.OverdueRental);
            ws.Cell(row, 7).Value = items.Sum(x => x.RemainingDebt);
            ws.Cell(row, 8).Value = items.Sum(x => x.OverdueRental + x.RemainingDebt);
            for (int col = 1; col <= 8; col++)
            {
                ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromHtml("#f0f4f8");
                ws.Cell(row, col).Style.Font.Bold = true;
                ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"BalanceReport_{DateTime.Today:yyyyMMdd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> CollectionReport(int? EmpCodeString, string? EmpSearch, int? companyId, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
            TempData.Keep();
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            var companiesList = companyIds.Any()
                ? await _context.CompanyInfos
                    .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync()
                : new List<CompanyInfo>();

            ViewBag.Companies = new SelectList(companiesList, "Id", "CompNameAr", companyId);

            // Default date range: last 7 days if nothing selected
            var fromDate = FromDateSearch.HasValue
                ? DateOnly.FromDateTime(FromDateSearch.Value)
                : DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
            var toDate = ToDateSearch.HasValue
                ? DateOnly.FromDateTime(ToDateSearch.Value)
                : DateOnly.FromDateTime(DateTime.Today);

            ViewData["FromDateFilter"] = FromDateSearch?.ToString("yyyy-MM-dd") ?? "";
            ViewData["ToDateFilter"] = ToDateSearch?.ToString("yyyy-MM-dd") ?? "";
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpFilter"] = EmpSearch;
            ViewData["CompanyFilter"] = companyId;

            // ── Bills query ──────────────────────────────────────────────
            var billQuery = _context.Bills
                .FromSqlRaw($"Select * from Bill where EmployeeId In (Select Id From EmployeeInfo where DeleteFlag = 0 and CompanyId IN ({companyIdsString}))")
                .Include(b => b.Employee).ThenInclude(e => e!.Company)
                .Include(b => b.Contract).ThenInclude(c => c!.Car)
                .Where(b => b.DeleteFlag == 0
                         && b.BillDate >= fromDate
                         && b.BillDate <= toDate
                         && b.Employee!.DeleteFlag == 0);

            if (companyId.HasValue)
                billQuery = (IOrderedQueryable<Bill>)billQuery.Where(b => b.Employee!.CompanyId == companyId.Value);
            if (EmpCodeString.HasValue)
                billQuery = (IOrderedQueryable<Bill>)billQuery.Where(b => b.Employee!.EmpCode == EmpCodeString.Value);
            if (!string.IsNullOrEmpty(EmpSearch))
                billQuery = (IOrderedQueryable<Bill>)billQuery.Where(b => b.Employee!.FullNameAr!.Contains(EmpSearch));

            var bills = await billQuery
                .OrderByDescending(b => b.BillDate)
                .Select(b => new BillCollectionItem
                {
                    BillNo = b.BillNo ?? 0,
                    BillDate = b.BillDate,
                    EmpCode = (int)(b.Employee!.EmpCode ?? 0),
                    EmployeeName = b.Employee.FullNameAr ?? "",
                    CompanyName = b.Employee.Company!.CompNameAr ?? "",
                    ContractNo = b.Contract!.ContractNo ?? "",
                    CarNo = b.Contract.Car!.CarNo ?? "",
                    Amount = b.BillPayed ?? 0
                })
                .ToListAsync();

            // ── DebitPayInfos query ──────────────────────────────────────
            var debitPayQuery = _context.DebitPayInfos
                .FromSqlRaw($"select * from DebitPayInfo where DebitInfoId in (Select Id from DebitInfo where EmpId in (Select Id from EmployeeInfo where DeleteFlag = 0 and CompanyId IN ({companyIdsString})))")
                .Include(d => d.DebitInfo).ThenInclude(di => di!.Emp).ThenInclude(e => e!.Company)
                .Include(d => d.DebitInfo).ThenInclude(di => di!.DebitType)
                .Where(d => d.DeleteFlag == 0
                         && d.DebitInfo!.Emp!.DeleteFlag == 0
                         && d.DebitPayDate >= fromDate
                         && d.DebitPayDate <= toDate);

            if (companyId.HasValue)
                debitPayQuery = (IOrderedQueryable<DebitPayInfo>)debitPayQuery.Where(d => d.DebitInfo!.Emp!.CompanyId == companyId.Value);
            if (EmpCodeString.HasValue)
                debitPayQuery = (IOrderedQueryable<DebitPayInfo>)debitPayQuery.Where(d => d.DebitInfo!.Emp!.EmpCode == EmpCodeString.Value);
            if (!string.IsNullOrEmpty(EmpSearch))
                debitPayQuery = (IOrderedQueryable<DebitPayInfo>)debitPayQuery.Where(d => d.DebitInfo!.Emp!.FullNameAr!.Contains(EmpSearch));

            var debitPayments = await debitPayQuery
                .OrderByDescending(d => d.DebitPayDate)
                .Select(d => new DebitPayCollectionItem
                {
                    DebitPayNo = d.DebitPayNo ?? 0,
                    PayDate = d.DebitPayDate,
                    EmpCode = (int)(d.DebitInfo!.Emp!.EmpCode ?? 0),
                    EmployeeName = d.DebitInfo.Emp.FullNameAr ?? "",
                    CompanyName = d.DebitInfo.Emp.Company!.CompNameAr ?? "",
                    DebitType = d.DebitInfo.DebitType!.DeffName ?? "",
                    Amount = d.DebitPayQty ?? 0
                })
                .ToListAsync();

            var viewModel = new CollectionReportViewModel
            {
                Bills = bills,
                DebitPayments = debitPayments
            };

            return View(viewModel);
        }

        public async Task<IActionResult> CollectionReportExcel(int? EmpCodeString, string? EmpSearch, int? companyId, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
            TempData.Keep();
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData!.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            var fromDate = FromDateSearch.HasValue ? DateOnly.FromDateTime(FromDateSearch.Value) : DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
            var toDate = ToDateSearch.HasValue ? DateOnly.FromDateTime(ToDateSearch.Value) : DateOnly.FromDateTime(DateTime.Today);

            var billQuery = _context.Bills
                .FromSqlRaw($"Select * from Bill where EmployeeId In (Select Id From EmployeeInfo where DeleteFlag = 0 and CompanyId IN ({companyIdsString}))")
                .Include(b => b.Employee).ThenInclude(e => e!.Company)
                .Include(b => b.Contract).ThenInclude(c => c!.Car)
                .Where(b => b.DeleteFlag == 0 && b.BillDate >= fromDate && b.BillDate <= toDate && b.Employee!.DeleteFlag == 0);

            if (companyId.HasValue) billQuery = (IOrderedQueryable<Bill>)billQuery.Where(b => b.Employee!.CompanyId == companyId.Value);
            if (EmpCodeString.HasValue) billQuery = (IOrderedQueryable<Bill>)billQuery.Where(b => b.Employee!.EmpCode == EmpCodeString.Value);
            if (!string.IsNullOrEmpty(EmpSearch)) billQuery = (IOrderedQueryable<Bill>)billQuery.Where(b => b.Employee!.FullNameAr!.Contains(EmpSearch));

            var bills = await billQuery.Select(b => new BillCollectionItem
            {
                EmpCode = (int)(b.Employee!.EmpCode ?? 0),
                EmployeeName = b.Employee.FullNameAr ?? "",
                Amount = b.BillPayed ?? 0
            }).ToListAsync();

            var debitQuery = _context.DebitPayInfos
                .FromSqlRaw($"select * from DebitPayInfo where DebitInfoId in (Select Id from DebitInfo where EmpId in (Select Id from EmployeeInfo where DeleteFlag = 0 and CompanyId IN ({companyIdsString})))")
                .Include(d => d.DebitInfo).ThenInclude(di => di!.Emp)
                .Where(d => d.DeleteFlag == 0 && d.DebitInfo!.Emp!.DeleteFlag == 0 && d.DebitPayDate >= fromDate && d.DebitPayDate <= toDate);

            if (companyId.HasValue) debitQuery = (IOrderedQueryable<DebitPayInfo>)debitQuery.Where(d => d.DebitInfo!.Emp!.CompanyId == companyId.Value);
            if (EmpCodeString.HasValue) debitQuery = (IOrderedQueryable<DebitPayInfo>)debitQuery.Where(d => d.DebitInfo!.Emp!.EmpCode == EmpCodeString.Value);
            if (!string.IsNullOrEmpty(EmpSearch)) debitQuery = (IOrderedQueryable<DebitPayInfo>)debitQuery.Where(d => d.DebitInfo!.Emp!.FullNameAr!.Contains(EmpSearch));

            var debits = await debitQuery.Select(d => new DebitPayCollectionItem
            {
                EmpCode = (int)(d.DebitInfo!.Emp!.EmpCode ?? 0),
                EmployeeName = d.DebitInfo.Emp.FullNameAr ?? "",
                Amount = d.DebitPayQty ?? 0
            }).ToListAsync();

            var billsByEmp = bills.GroupBy(b => new { b.EmpCode, b.EmployeeName }).Select(g => new { g.Key.EmpCode, g.Key.EmployeeName, BillsTotal = g.Sum(x => x.Amount) }).ToList();
            var debitsByEmp = debits.GroupBy(d => new { d.EmpCode, d.EmployeeName }).Select(g => new { g.Key.EmpCode, g.Key.EmployeeName, DebitsTotal = g.Sum(x => x.Amount) }).ToList();
            var allCodes = billsByEmp.Select(b => b.EmpCode).Union(debitsByEmp.Select(d => d.EmpCode)).Distinct().OrderBy(c => c).ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("تقرير الإيجار اليومي");
            ws.RightToLeft = true;

            ws.Cell(1, 1).Value = "رقم الموظف";
            ws.Cell(1, 2).Value = "اسم الموظف";
            ws.Cell(1, 3).Value = "الإيداع اليومي";
            ws.Cell(1, 4).Value = "تحصيل الديون";
            ws.Cell(1, 5).Value = "الإجمالي";

            var headerRow = ws.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#1565c0");
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            decimal totalBills = 0, totalDebits = 0;
            foreach (var code in allCodes)
            {
                var b = billsByEmp.FirstOrDefault(x => x.EmpCode == code);
                var d = debitsByEmp.FirstOrDefault(x => x.EmpCode == code);
                var bt = b?.BillsTotal ?? 0m;
                var dt = d?.DebitsTotal ?? 0m;
                ws.Cell(row, 1).Value = code;
                ws.Cell(row, 2).Value = b?.EmployeeName ?? d?.EmployeeName ?? "";
                ws.Cell(row, 3).Value = bt;
                ws.Cell(row, 4).Value = dt;
                ws.Cell(row, 5).Value = bt + dt;
                totalBills += bt;
                totalDebits += dt;
                row++;
            }

            ws.Cell(row, 1).Value = "الإجمالي";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = totalBills;
            ws.Cell(row, 4).Value = totalDebits;
            ws.Cell(row, 5).Value = totalBills + totalDebits;
            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.Columns(3, 5).Style.NumberFormat.Format = "#,##0.000";
            ws.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"تقرير_التحصيل_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> AccountStatement(int? EmpCodeString, string? EmpSearch, int? CarCodeString, string? CarSearch, int? companyId, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
            TempData.Keep();
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            var companiesList = companyIds.Any()
                ? await _context.CompanyInfos
                    .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync()
                : new List<CompanyInfo>();

            ViewBag.Companies = new SelectList(companiesList, "Id", "CompNameAr", companyId);

            var today = DateOnly.FromDateTime(DateTime.Today);
            var viewModel = new AccountStatementViewModel();

            DateOnly? fromDate = FromDateSearch.HasValue ? DateOnly.FromDateTime(FromDateSearch.Value) : null;
            DateOnly? toDate = ToDateSearch.HasValue ? DateOnly.FromDateTime(ToDateSearch.Value) : null;

            if (EmpCodeString.HasValue || !string.IsNullOrEmpty(EmpSearch))
            {
                var employeeQuery = _context.EmployeeInfos
                    .Include(e => e.Company)
                    .Where(e => e.DeleteFlag == 0 && companyIds.Contains(e.CompanyId ?? 0));

                if (EmpCodeString.HasValue)
                    employeeQuery = employeeQuery.Where(e => e.EmpCode == EmpCodeString);
                if (!string.IsNullOrEmpty(EmpSearch))
                    employeeQuery = employeeQuery.Where(e => e.FullNameAr!.Contains(EmpSearch));
                if (companyId.HasValue)
                    employeeQuery = employeeQuery.Where(e => e.CompanyId == companyId.Value);

                // نحتاج موظفًا واحدًا فقط لعرض كشف حسابه؛ إذا طابق البحث أكثر من موظف نتجاهل حتى يتم تحديد الكود
                var matchingEmployees = await employeeQuery.Take(2).ToListAsync();
                var employee = matchingEmployees.Count == 1 ? matchingEmployees[0] : null;

                if (employee != null)
                {
                    var contractsQuery = _context.Contracts
                        .Include(c => c.Car)
                        .Where(c => c.DeleteFlag == 0 && c.EmployeeId == employee.Id);

                    if (CarCodeString.HasValue)
                        contractsQuery = contractsQuery.Where(c => c.Car!.CarCode == CarCodeString);
                    if (!string.IsNullOrEmpty(CarSearch))
                        contractsQuery = contractsQuery.Where(c => c.Car!.CarNo!.Contains(CarSearch));

                    var contracts = await contractsQuery.ToListAsync();

                    // عند اختيار الموظف لأول مرة (بدون تواريخ محددة يدويًا):
                    // من بداية العقد حتى تاريخ اليوم، أو حتى تاريخ نهاية التعاقد إذا كان قبل تاريخ اليوم
                    if (contracts.Any())
                    {
                        if (!fromDate.HasValue)
                            fromDate = contracts.Min(c => c.StartDate);

                        if (!toDate.HasValue)
                        {
                            var contractEnd = contracts.Max(c => c.EndDate);
                            toDate = (contractEnd.HasValue && contractEnd.Value < today) ? contractEnd.Value : today;
                        }
                    }

                    fromDate ??= today;
                    toDate ??= today;

                    var contractIds = contracts.Select(c => c.Id).ToList();

                    var charges = await _context.ContractDetails
                        .Include(cd => cd.Contract).ThenInclude(c => c!.Car)
                        .Where(cd => cd.DeleteFlag == 0
                            && contractIds.Contains(cd.ContractId ?? 0)
                            && cd.DailyCreditDate.HasValue
                            && cd.DailyCreditDate >= fromDate
                            && cd.DailyCreditDate <= toDate)
                        .ToListAsync();

                    var billsQuery = _context.Bills
                        .Where(b => b.DeleteFlag == 0
                            && b.EmployeeId == employee.Id
                            && b.BillDate.HasValue
                            && b.BillDate >= fromDate
                            && b.BillDate <= toDate);
                    if (contractIds.Any())
                        billsQuery = billsQuery.Where(b => contractIds.Contains(b.ContractId ?? 0));

                    var bills = await billsQuery.ToListAsync();

                    var debits = await _context.DebitInfos
                        .Include(d => d.DebitType)
                        .Where(d => d.DeleteFlag == 0
                            && d.EmpId == employee.Id
                            && d.DebitDate.HasValue
                            && d.DebitDate >= fromDate
                            && d.DebitDate <= toDate)
                        .ToListAsync();

                    var debitPays = await _context.DebitPayInfos
                        .Include(p => p.DebitInfo)
                        .Where(p => p.DeleteFlag == 0
                            && p.DebitInfo!.EmpId == employee.Id
                            && p.DebitPayDate.HasValue
                            && p.DebitPayDate >= fromDate
                            && p.DebitPayDate <= toDate)
                        .ToListAsync();

                    int empCode = employee.EmpCode ?? 0;
                    string employeeName = employee.FullNameAr ?? "";
                    string companyName = employee.Company?.CompNameAr ?? "";

                    var items = new List<AccountStatementItem>();

                    foreach (var c in charges)
                    {
                        var amount = (c.DailyCredit ?? 0) + (c.CarCredit ?? 0);
                        if (amount == 0) continue;

                        items.Add(new AccountStatementItem
                        {
                            EmpCode = empCode,
                            EmployeeName = employeeName,
                            CompanyName = companyName,
                            Date = c.DailyCreditDate!.Value,
                            Rent = amount
                        });
                    }

                    foreach (var b in bills)
                    {
                        items.Add(new AccountStatementItem
                        {
                            EmpCode = empCode,
                            EmployeeName = employeeName,
                            CompanyName = companyName,
                            Date = b.BillDate!.Value,
                            RentPay = b.BillPayed ?? 0
                        });
                    }

                    foreach (var d in debits)
                    {
                        items.Add(new AccountStatementItem
                        {
                            EmpCode = empCode,
                            EmployeeName = employeeName,
                            CompanyName = companyName,
                            Date = d.DebitDate!.Value,
                            Debt = d.DebitQty ?? 0
                        });
                    }

                    foreach (var p in debitPays)
                    {
                        items.Add(new AccountStatementItem
                        {
                            EmpCode = empCode,
                            EmployeeName = employeeName,
                            CompanyName = companyName,
                            Date = p.DebitPayDate!.Value,
                            DebtPay = p.DebitPayQty ?? 0
                        });
                    }

                    items = items.OrderBy(i => i.Date).ToList();

                    decimal balance = 0;
                    foreach (var item in items)
                    {
                        item.PreviousBalance = balance;
                        balance += item.Rent + item.Debt - item.RentPay - item.DebtPay;
                        item.CurrentBalance = balance;
                    }

                    viewModel.HasEmployee = true;
                    viewModel.EmpCode = empCode;
                    viewModel.EmployeeName = employeeName;
                    viewModel.MobileNo = employee.MobiileNo ?? "";
                    viewModel.CompanyName = companyName;
                    viewModel.Items = items;
                }
            }

            viewModel.FromDate = fromDate;
            viewModel.ToDate = toDate;

            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpFilter"] = EmpSearch;
            ViewData["CarCodeFilter"] = CarCodeString;
            ViewData["CarFilter"] = CarSearch;
            ViewData["CompanyFilter"] = companyId;
            ViewData["FromDateFilter"] = fromDate?.ToString("yyyy-MM-dd") ?? "";
            ViewData["ToDateFilter"] = toDate?.ToString("yyyy-MM-dd") ?? "";

            return View(viewModel);
        }
    }
}