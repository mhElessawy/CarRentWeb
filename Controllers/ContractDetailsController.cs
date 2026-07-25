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

            var fromDate = FromDateSearch.HasValue ? DateOnly.FromDateTime(FromDateSearch.Value) : DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            var toDate = ToDateSearch.HasValue ? DateOnly.FromDateTime(ToDateSearch.Value) : DateOnly.FromDateTime(DateTime.Today);

            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpFilter"] = EmpSearch;
            ViewData["CarCodeFilter"] = CarCodeString;
            ViewData["CarFilter"] = CarSearch;
            ViewData["CompanyFilter"] = companyId;
            ViewData["FromDateFilter"] = fromDate.ToString("yyyy-MM-dd");
            ViewData["ToDateFilter"] = toDate.ToString("yyyy-MM-dd");

            bool hasSearch = EmpCodeString.HasValue || !string.IsNullOrEmpty(EmpSearch) || CarCodeString.HasValue || !string.IsNullOrEmpty(CarSearch);
            if (!hasSearch)
                return View(new AccountStatementViewModel());

            var viewModel = await BuildAccountStatementAsync(EmpCodeString, EmpSearch, CarCodeString, CarSearch, companyId, companyIdsString, fromDate, toDate);

            return View(viewModel);
        }

        public async Task<IActionResult> AccountStatementExcel(int? EmpCodeString, string? EmpSearch, int? CarCodeString, string? CarSearch, int? companyId, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
            TempData.Keep();
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData!.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            var fromDate = FromDateSearch.HasValue ? DateOnly.FromDateTime(FromDateSearch.Value) : DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            var toDate = ToDateSearch.HasValue ? DateOnly.FromDateTime(ToDateSearch.Value) : DateOnly.FromDateTime(DateTime.Today);

            var viewModel = await BuildAccountStatementAsync(EmpCodeString, EmpSearch, CarCodeString, CarSearch, companyId, companyIdsString, fromDate, toDate);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("كشف الحساب");
            ws.RightToLeft = true;

            ws.Cell(1, 1).Value = "كشف حساب السائق";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(1, 1, 1, 11).Merge();

            ws.Cell(2, 1).Value = $"من {fromDate:yyyy/MM/dd} إلى {toDate:yyyy/MM/dd}";
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(2, 1, 2, 11).Merge();

            int headerRow = 4;
            string[] headers = { "#", "رقم السائق", "اسم السائق", "التاريخ", "الإيجار", "الدين", "دفع الإيجار", "دفع الدين", "الشركة", "الرصيد السابق", "الرصيد الحالي" };
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

            int row = headerRow + 1;
            int num = 1;
            foreach (var item in viewModel.Rows)
            {
                ws.Cell(row, 1).Value = num++;
                ws.Cell(row, 2).Value = item.EmpCode;
                ws.Cell(row, 3).Value = item.EmployeeName;
                ws.Cell(row, 4).Value = item.TxnDate.ToString("yyyy/MM/dd");
                ws.Cell(row, 5).Value = item.Rent;
                ws.Cell(row, 6).Value = item.Debt;
                ws.Cell(row, 7).Value = item.RentPaid;
                ws.Cell(row, 8).Value = item.DebtPaid;
                ws.Cell(row, 9).Value = item.CompanyName;
                ws.Cell(row, 10).Value = item.PreviousBalance;
                ws.Cell(row, 11).Value = item.CurrentBalance;

                for (int col = 1; col <= 11; col++)
                {
                    ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                if (row % 2 == 0)
                    ws.Range(row, 1, row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");

                row++;
            }

            ws.Cell(row, 1).Value = "الإجمالي";
            ws.Range(row, 1, row, 4).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 5).Value = viewModel.TotalRentInPeriod;
            ws.Cell(row, 6).Value = viewModel.TotalDebtInPeriod;
            ws.Cell(row, 7).Value = viewModel.TotalRentPaidInPeriod;
            ws.Cell(row, 8).Value = viewModel.TotalDebtPaidInPeriod;
            for (int col = 1; col <= 11; col++)
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

            string fileName = $"AccountStatement_{DateTime.Today:yyyyMMdd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task<AccountStatementViewModel> BuildAccountStatementAsync(
            int? empCodeString, string? empSearch, int? carCodeString, string? carSearch,
            int? companyId, string companyIdsString, DateOnly fromDate, DateOnly toDate)
        {
            var viewModel = new AccountStatementViewModel { HasSearched = true };

            List<int> employeeIds;

            if (carCodeString.HasValue || !string.IsNullOrEmpty(carSearch))
            {
                var carQuery = _context.CarInfos
                    .FromSqlRaw($"SELECT * FROM CarInfo WHERE DeleteFlag = 0 AND CompanyId IN ({companyIdsString})");

                if (companyId.HasValue)
                    carQuery = carQuery.Where(c => c.CompanyId == companyId.Value);
                if (carCodeString.HasValue)
                    carQuery = carQuery.Where(c => c.CarCode == carCodeString.Value);
                if (!string.IsNullOrEmpty(carSearch))
                    carQuery = carQuery.Where(c => c.CarNo!.Contains(carSearch));

                var carIds = await carQuery.Select(c => c.Id).ToListAsync();

                var empFromCarQuery = _context.Contracts
                    .Where(c => c.DeleteFlag == 0 && c.CarId.HasValue && carIds.Contains(c.CarId.Value));

                if (empCodeString.HasValue)
                    empFromCarQuery = empFromCarQuery.Where(c => c.Employee!.EmpCode == empCodeString.Value);
                if (!string.IsNullOrEmpty(empSearch))
                    empFromCarQuery = empFromCarQuery.Where(c => c.Employee!.FullNameAr!.Contains(empSearch));

                employeeIds = await empFromCarQuery
                    .Where(c => c.EmployeeId.HasValue)
                    .Select(c => c.EmployeeId!.Value)
                    .Distinct()
                    .ToListAsync();
            }
            else
            {
                var empQuery = _context.EmployeeInfos
                    .FromSqlRaw($"SELECT * FROM EmployeeInfo WHERE DeleteFlag = 0 AND CompanyId IN ({companyIdsString})");

                if (companyId.HasValue)
                    empQuery = empQuery.Where(e => e.CompanyId == companyId.Value);
                if (empCodeString.HasValue)
                    empQuery = empQuery.Where(e => e.EmpCode == empCodeString.Value);
                if (!string.IsNullOrEmpty(empSearch))
                    empQuery = empQuery.Where(e => e.FullNameAr!.Contains(empSearch));

                employeeIds = await empQuery.Select(e => e.Id).ToListAsync();
            }

            if (!employeeIds.Any())
                return viewModel;

            var employees = await _context.EmployeeInfos
                .Include(e => e.Company)
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync();

            if (employees.Count == 1)
            {
                viewModel.EmpCode = employees[0].EmpCode;
                viewModel.EmployeeName = employees[0].FullNameAr;
                viewModel.MobileNo = employees[0].MobiileNo;
                viewModel.CompanyName = employees[0].Company?.CompNameAr;
            }

            var empLookup = employees.ToDictionary(e => e.Id, e => e);

            // Contracts + rent schedule (full history, drives the "contract details" summary section)
            var contracts = await _context.Contracts
                .Include(c => c.Car)
                .Where(c => c.DeleteFlag == 0 && c.EmployeeId.HasValue && employeeIds.Contains(c.EmployeeId.Value))
                .OrderByDescending(c => c.ContractDate)
                .ToListAsync();

            var contractIds = contracts.Select(c => c.Id).ToList();
            var contractEmpMap = contracts.ToDictionary(c => c.Id, c => c.EmployeeId ?? 0);

            var contractDetailsAll = await _context.ContractDetails
                .Where(cd => cd.DeleteFlag == 0 && cd.ContractId.HasValue && contractIds.Contains(cd.ContractId.Value))
                .ToListAsync();

            foreach (var c in contracts)
            {
                var details = contractDetailsAll.Where(cd => cd.ContractId == c.Id).ToList();
                var total = details.Sum(cd => (cd.DailyCredit ?? 0) + (cd.CarCredit ?? 0));
                var paid = details.Where(cd => cd.Status == 3).Sum(cd => (cd.DailyCredit ?? 0) + (cd.CarCredit ?? 0));

                viewModel.Contracts.Add(new AccountStatementContractItem
                {
                    ContractNo = c.ContractNo,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    CarNo = c.Car?.CarNo,
                    DailyCredit = c.DailyCredit,
                    TotalCost = c.TotalCost,
                    Status = c.Status,
                    TotalRent = total,
                    PaidRent = paid
                });
            }

            // Debts (full history, drives the "contract details" summary section)
            var debitInfosAll = await _context.DebitInfos
                .Where(d => d.DeleteFlag == 0 && d.EmpId.HasValue && employeeIds.Contains(d.EmpId.Value))
                .ToListAsync();

            viewModel.TotalDebt = debitInfosAll.Sum(d => d.DebitQty ?? 0);
            viewModel.PaidDebt = debitInfosAll.Sum(d => d.DebitPayed ?? 0);
            viewModel.RemainingDebt = debitInfosAll.Sum(d => d.DebitRemaining ?? 0);

            var debitInfoEmpMap = debitInfosAll.ToDictionary(d => d.Id, d => d.EmpId ?? 0);
            var debitInfoIds = debitInfosAll.Select(d => d.Id).ToList();

            // Rent payments (daily-rental bills + monthly-rental credit bills)
            var bills = await _context.Bills
                .Where(b => b.DeleteFlag == 0 && b.EmployeeId.HasValue && employeeIds.Contains(b.EmployeeId.Value))
                .ToListAsync();

            var creditBills = await _context.CreditBills
                .Where(b => b.DeleteFlag == 0 && b.EmployeeId.HasValue && employeeIds.Contains(b.EmployeeId.Value))
                .ToListAsync();

            // Debt payments
            var debitPayInfosAll = await _context.DebitPayInfos
                .Where(dp => dp.DeleteFlag == 0 && dp.DebitInfoId.HasValue && debitInfoIds.Contains(dp.DebitInfoId.Value))
                .ToListAsync();

            // ── Build a per-employee, per-day ledger (rent charge / debt charge / rent paid / debt paid) ──
            var txnMap = new Dictionary<int, Dictionary<DateOnly, (decimal rent, decimal debt, decimal rentPaid, decimal debtPaid)>>();

            void AddTxn(int empId, DateOnly date, decimal rent = 0, decimal debt = 0, decimal rentPaid = 0, decimal debtPaid = 0)
            {
                if (empId == 0) return;
                if (!txnMap.TryGetValue(empId, out var dateMap))
                {
                    dateMap = new Dictionary<DateOnly, (decimal, decimal, decimal, decimal)>();
                    txnMap[empId] = dateMap;
                }
                dateMap.TryGetValue(date, out var v);
                dateMap[date] = (v.Item1 + rent, v.Item2 + debt, v.Item3 + rentPaid, v.Item4 + debtPaid);
            }

            foreach (var cd in contractDetailsAll)
            {
                if (!cd.DailyCreditDate.HasValue) continue;
                var empId = contractEmpMap.TryGetValue(cd.ContractId ?? 0, out var eid) ? eid : 0;
                AddTxn(empId, cd.DailyCreditDate.Value, rent: (cd.DailyCredit ?? 0) + (cd.CarCredit ?? 0));
            }

            foreach (var d in debitInfosAll)
            {
                if (!d.DebitDate.HasValue) continue;
                AddTxn(d.EmpId ?? 0, d.DebitDate.Value, debt: d.DebitQty ?? 0);
            }

            foreach (var b in bills)
            {
                if (!b.BillDate.HasValue) continue;
                AddTxn(b.EmployeeId ?? 0, b.BillDate.Value, rentPaid: b.BillPayed ?? 0);
            }

            foreach (var b in creditBills)
            {
                if (!b.CreditBillDate.HasValue) continue;
                AddTxn(b.EmployeeId ?? 0, b.CreditBillDate.Value, rentPaid: b.CreditBillPayed ?? 0);
            }

            foreach (var dp in debitPayInfosAll)
            {
                if (!dp.DebitPayDate.HasValue) continue;
                var empId = debitInfoEmpMap.TryGetValue(dp.DebitInfoId ?? 0, out var eid) ? eid : 0;
                AddTxn(empId, dp.DebitPayDate.Value, debtPaid: dp.DebitPayQty ?? 0);
            }

            foreach (var empId in txnMap.Keys.OrderBy(id => empLookup.TryGetValue(id, out var e) ? e.EmpCode ?? 0 : 0))
            {
                var emp = empLookup.TryGetValue(empId, out var e2) ? e2 : null;
                var empCode = emp?.EmpCode ?? 0;
                var empName = emp?.FullNameAr ?? "";
                var compName = emp?.Company?.CompNameAr ?? "";

                var dateMap = txnMap[empId];

                var openingBalance = dateMap
                    .Where(kv => kv.Key < fromDate)
                    .Sum(kv => kv.Value.rent + kv.Value.debt - kv.Value.rentPaid - kv.Value.debtPaid);

                var runningBalance = openingBalance;

                var datesInRange = dateMap.Keys.Where(d => d >= fromDate && d <= toDate).OrderBy(d => d).ToList();

                foreach (var date in datesInRange)
                {
                    var v = dateMap[date];
                    var previousBalance = runningBalance;
                    runningBalance += v.rent + v.debt - v.rentPaid - v.debtPaid;

                    viewModel.Rows.Add(new AccountStatementRow
                    {
                        EmpCode = empCode,
                        EmployeeName = empName,
                        CompanyName = compName,
                        TxnDate = date,
                        Rent = v.rent,
                        Debt = v.debt,
                        RentPaid = v.rentPaid,
                        DebtPaid = v.debtPaid,
                        PreviousBalance = previousBalance,
                        CurrentBalance = runningBalance
                    });
                }
            }

            return viewModel;
        }
    }
}