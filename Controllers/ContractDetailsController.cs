using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;


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
        public async Task<IActionResult> IndexReportAudit(
            DateOnly? fromDate,
            DateOnly? toDate,
            int? companyId,
            string? empCode,
            string? empName)
        {
            TempData.Keep();

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            var companies = companyIds.Any()
                ? await _context.CompanyInfos
                    .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync()
                : new List<CompanyInfo>();

            ViewBag.Companies = new SelectList(companies, "Id", "CompNameAr", companyId);
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.EmpCode = empCode;
            ViewBag.EmpName = empName;

            var query = _context.ContractDetails
                .FromSqlRaw($"select * from ContractDetails where ContractId In (Select Id from Contract where DeleteFlag = 0 and EmployeeId In (Select Id From EmployeeInfo where CompanyId IN ({companyIdsString})))")
                .Include(c => c.Contract)
                    .ThenInclude(c => c!.Employee)
                        .ThenInclude(e => e!.Company)
                .Where(a => a.DeleteFlag == 0
                            && (a.Status == 3 || a.Status == 0)
                            && (a.DailyCredit != 0 || a.CarCredit != 0));

            if (fromDate.HasValue)
                query = query.Where(a => a.DailyCreditDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.DailyCreditDate <= toDate.Value);

            if (companyId.HasValue)
                query = query.Where(e => e.Contract!.Employee!.CompanyId == companyId.Value);

            if (!string.IsNullOrWhiteSpace(empCode) && int.TryParse(empCode.Trim(), out int empCodeInt))
                query = query.Where(e => e.Contract!.Employee!.EmpCode == empCodeInt);

            if (!string.IsNullOrWhiteSpace(empName))
                query = query.Where(e => e.Contract!.Employee!.FullNameAr!.Contains(empName.Trim()));

            var result = await query
                .Where(c => c.Contract != null &&
                            c.Contract.Employee != null &&
                            c.Contract.Employee.EmpCode != null)
                .GroupBy(c => c.Contract!.Employee!.Id)
                .Select(g => new ContractDetailsSumation
                {
                    EmployeeId = g.Key,
                    EmpCode = (int)g.First().Contract!.Employee!.EmpCode!,
                    MobileNo = g.First().Contract!.Employee!.MobiileNo ?? "",
                    EmployeeName = g.First().Contract!.Employee!.FullNameAr ?? "",
                    CompanyName = g.First().Contract!.Employee!.Company!.CompNameAr ?? "",
                    TotalDailyCredit = (decimal)g.Where(c => c.Status == 3).Sum(c => c.DailyCredit),
                    TotalCarCredit = (decimal)g.Where(c => c.Status == 3).Sum(c => c.CarCredit),
                    RemainingDebt = (decimal)g.Where(c => c.Status == 0).Sum(c => (c.DailyCredit ?? 0) + (c.CarCredit ?? 0)),
                    OverdueRental = (decimal)g.Where(c => c.Status == 0 && c.DailyCreditDate < DateOnly.FromDateTime(DateTime.Today)).Sum(c => c.DailyCredit ?? 0)
                })
                .OrderBy(x => x.EmpCode)
                .ToListAsync();

            return View(result);


        }

        public async Task<IActionResult> BalanceReport(
            int? companyId,
            string? empCode,
            string? empName,
            DateOnly? fromDate,
            DateOnly? toDate)
        {
            TempData.Keep();
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Where(x => int.TryParse(x.Trim(), out _)).Select(x => int.Parse(x.Trim())).ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            var companies = companyIds.Any()
                ? await _context.CompanyInfos
                    .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync()
                : new List<CompanyInfo>();

            ViewBag.Companies = new SelectList(companies, "Id", "CompNameAr", companyId);
            ViewData["EmpCodeFilter"] = empCode;
            ViewData["EmpNameFilter"] = empName;

            var query = _context.ContractDetails
                .FromSqlRaw($"select * from ContractDetails where ContractId In (Select Id from Contract where DeleteFlag = 0 and EmployeeId In (Select Id From EmployeeInfo where CompanyId IN ({companyIdsString})))")
                .Include(c => c.Contract)
                    .ThenInclude(c => c!.Employee)
                        .ThenInclude(e => e!.Company)
                .Where(a => a.DeleteFlag == 0 && (a.DailyCredit != 0 || a.CarCredit != 0));

            if (companyId.HasValue)
                query = query.Where(e => e.Contract!.Employee!.CompanyId == companyId.Value);

            if (!string.IsNullOrWhiteSpace(empCode) && int.TryParse(empCode.Trim(), out int empCodeInt))
                query = query.Where(e => e.Contract!.Employee!.EmpCode == empCodeInt);

            if (!string.IsNullOrWhiteSpace(empName))
                query = query.Where(e => e.Contract!.Employee!.FullNameAr!.Contains(empName.Trim()));

            var today = DateOnly.FromDateTime(DateTime.Today);

            var result = await query
                .Where(c => c.Contract != null && c.Contract.Employee != null && c.Contract.Employee.EmpCode != null)
                .GroupBy(c => c.Contract!.Employee!.Id)
                .Select(g => new ContractDetailsSumation
                {
                    EmployeeId = g.Key,
                    EmpCode = (int)g.First().Contract!.Employee!.EmpCode!,
                    MobileNo = g.First().Contract!.Employee!.MobiileNo ?? "",
                    EmployeeName = g.First().Contract!.Employee!.FullNameAr ?? "",
                    CompanyName = g.First().Contract!.Employee!.Company!.CompNameAr ?? "",
                    TotalDailyCredit = (decimal)g.Sum(c => c.DailyCredit ?? 0),
                    TotalCarCredit = (decimal)g.Sum(c => c.CarCredit ?? 0),
                    RemainingDebt = (decimal)g.Where(c => c.Status == 0).Sum(c => (c.DailyCredit ?? 0) + (c.CarCredit ?? 0)),
                    OverdueRental = (decimal)g.Where(c => c.Status == 0 && c.DailyCreditDate < today).Sum(c => c.DailyCredit ?? 0)
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
    }
}