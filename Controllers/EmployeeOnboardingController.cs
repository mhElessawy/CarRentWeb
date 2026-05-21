using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRentWeb.Data;
using CarRentWeb.Models;
using CarRentWeb.Models.MyModel;

namespace CarRentWeb.Controllers
{
    public class EmployeeOnboardingController : Controller
    {
        private readonly CarRentWebContext _context;

        public EmployeeOnboardingController(CarRentWebContext context)
        {
            _context = context;
        }

        // GET: EmployeeOnboarding — list employees with progress
        public async Task<IActionResult> Index(int? searchCode, string? nameSearch, int? companyId, string? statusFilter)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            TempData.Keep();

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString() ?? "";
            var companyIds = userCompanyData.Split(',')
                .Where(x => int.TryParse(x.Trim(), out _))
                .Select(x => int.Parse(x.Trim()))
                .ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            ViewBag.Companies = new SelectList(
                await _context.CompanyInfos
                    .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync(),
                "Id", "CompNameAr", companyId);

            var query = _context.EmployeeInfos
                .FromSqlRaw($"SELECT * FROM EmployeeInfo WHERE DeleteFlag = 0 AND CompanyId IN ({companyIdsString})")
                .Include(e => e.Company)
                .Where(e => e.DeleteFlag == 0)
                .AsQueryable();

            if (searchCode.HasValue)
                query = query.Where(e => e.EmpCode == searchCode.Value);

            if (!string.IsNullOrEmpty(nameSearch))
                query = query.Where(e =>
                    e.FullNameAr!.Contains(nameSearch) ||
                    e.FullNameEn!.Contains(nameSearch));

            if (companyId.HasValue)
                query = query.Where(e => e.CompanyId == companyId.Value);

            var employees = await query.OrderBy(e => e.EmpCode).ToListAsync();

            // Get progress counts per employee
            var totalSteps = await _context.DriverOnboardingSteps.CountAsync(s => s.IsActive);
            var progressData = await _context.EmployeeOnboardingProgresses
                .Where(p => p.IsCompleted)
                .GroupBy(p => p.EmployeeId)
                .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.EmployeeId, x => x.Count);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                employees = statusFilter switch
                {
                    "completed" => employees.Where(e => totalSteps > 0 && progressData.ContainsKey(e.Id) && progressData[e.Id] == totalSteps).ToList(),
                    "notcompleted" => employees.Where(e => !(totalSteps > 0 && progressData.ContainsKey(e.Id) && progressData[e.Id] == totalSteps)).ToList(),
                    _ => employees
                };
            }

            ViewBag.TotalSteps = totalSteps;
            ViewBag.ProgressData = progressData;
            ViewBag.SearchCode = searchCode;
            ViewBag.NameSearch = nameSearch;
            ViewBag.StatusFilter = statusFilter;

            return View(employees);
        }

        // GET: EmployeeOnboarding/Progress/5
        public async Task<IActionResult> Progress(int id)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            TempData.Keep();

            var employee = await _context.EmployeeInfos
                .Include(e => e.Company)
                .Include(e => e.Nationality)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            var allSteps = await _context.DriverOnboardingSteps
                .Where(s => s.IsActive)
                .OrderBy(s => s.StepOrder)
                .ThenBy(s => s.Id)
                .ToListAsync();

            var existingProgress = await _context.EmployeeOnboardingProgresses
                .Where(p => p.EmployeeId == id)
                .ToDictionaryAsync(p => p.StepId);

            var viewModel = new EmployeeOnboardingViewModel
            {
                Employee = employee,
                Steps = allSteps.Select(s => new StepProgressItem
                {
                    StepId = s.Id,
                    StepName = s.StepName,
                    Description = s.Description,
                    StepOrder = s.StepOrder,
                    Location = s.Location,
                    Jeha = s.Jeha,
                    IsCompleted = existingProgress.ContainsKey(s.Id) && existingProgress[s.Id].IsCompleted,
                    CompletedDate = existingProgress.ContainsKey(s.Id) ? existingProgress[s.Id].CompletedDate : null,
                    Notes = existingProgress.ContainsKey(s.Id) ? existingProgress[s.Id].Notes : null
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: EmployeeOnboarding/SaveProgress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProgress(int employeeId, int[]? completedStepIds, Dictionary<int, string>? stepNotes)
        {
            completedStepIds ??= Array.Empty<int>();
            stepNotes ??= new Dictionary<int, string>();

            var allSteps = await _context.DriverOnboardingSteps
                .Where(s => s.IsActive)
                .Select(s => s.Id)
                .ToListAsync();

            var existingProgress = await _context.EmployeeOnboardingProgresses
                .Where(p => p.EmployeeId == employeeId)
                .ToDictionaryAsync(p => p.StepId);

            foreach (var stepId in allSteps)
            {
                var isCompleted = completedStepIds.Contains(stepId);
                stepNotes.TryGetValue(stepId, out var note);

                if (existingProgress.TryGetValue(stepId, out var progress))
                {
                    if (isCompleted && !progress.IsCompleted)
                        progress.CompletedDate = DateOnly.FromDateTime(DateTime.Today);
                    else if (!isCompleted)
                        progress.CompletedDate = null;

                    progress.IsCompleted = isCompleted;
                    progress.Notes = note?.Trim();
                }
                else
                {
                    _context.EmployeeOnboardingProgresses.Add(new EmployeeOnboardingProgress
                    {
                        EmployeeId = employeeId,
                        StepId = stepId,
                        IsCompleted = isCompleted,
                        CompletedDate = isCompleted ? DateOnly.FromDateTime(DateTime.Today) : null,
                        Notes = note?.Trim()
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حفظ التقدم بنجاح";
            return RedirectToAction(nameof(Progress), new { id = employeeId });
        }

        // GET: EmployeeOnboarding/Schedule
        public async Task<IActionResult> Schedule(int? companyId, string? nameSearch)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            TempData.Keep();

            var userCompanyData = HttpContext.Session.GetString("UserCompanyData") ?? "";
            var companyIds = userCompanyData.Split(',')
                .Where(x => int.TryParse(x.Trim(), out _))
                .Select(x => int.Parse(x.Trim()))
                .ToList();
            var companyIdsString = companyIds.Any() ? string.Join(",", companyIds) : "0";

            ViewBag.Companies = new SelectList(
                await _context.CompanyInfos
                    .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync(),
                "Id", "CompNameAr", companyId);

            var allSteps = await _context.DriverOnboardingSteps
                .Where(s => s.IsActive)
                .OrderBy(s => s.StepOrder)
                .ThenBy(s => s.Id)
                .ToListAsync();

            if (!allSteps.Any())
            {
                ViewBag.NameSearch = nameSearch;
                ViewBag.CompanyId = companyId;
                return View(new List<EmployeeScheduleItem>());
            }

            var query = _context.EmployeeInfos
                .FromSqlRaw($"SELECT * FROM EmployeeInfo WHERE DeleteFlag = 0 AND CompanyId IN ({companyIdsString})")
                .Include(e => e.Company)
                .Where(e => e.DeleteFlag == 0)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nameSearch))
                query = query.Where(e => e.FullNameAr!.Contains(nameSearch) || e.FullNameEn!.Contains(nameSearch));

            if (companyId.HasValue)
                query = query.Where(e => e.CompanyId == companyId.Value);

            var employees = await query.OrderBy(e => e.EmpCode).ToListAsync();
            var employeeIds = employees.Select(e => e.Id).ToList();

            var allProgress = await _context.EmployeeOnboardingProgresses
                .Where(p => employeeIds.Contains(p.EmployeeId))
                .ToListAsync();

            var totalCount = allSteps.Count;
            var scheduleItems = new List<EmployeeScheduleItem>();

            foreach (var emp in employees)
            {
                var empProgress = allProgress.Where(p => p.EmployeeId == emp.Id).ToList();
                var completedIds = empProgress.Where(p => p.IsCompleted).Select(p => p.StepId).ToHashSet();
                var completedCount = completedIds.Count;

                if (completedCount == totalCount) continue;

                var nextStep = allSteps.FirstOrDefault(s => !completedIds.Contains(s.Id));
                if (nextStep == null) continue;

                var progressRecord = empProgress.FirstOrDefault(p => p.StepId == nextStep.Id);

                scheduleItems.Add(new EmployeeScheduleItem
                {
                    EmployeeId = emp.Id,
                    FullNameAr = emp.FullNameAr ?? "",
                    EmpCode = emp.EmpCode,
                    CompanyName = emp.Company?.CompNameAr,
                    NextStepId = nextStep.Id,
                    NextStepName = nextStep.StepName ?? "",
                    NextStepJeha = nextStep.Jeha,
                    NextStepLocation = nextStep.Location,
                    CompletedCount = completedCount,
                    TotalCount = totalCount,
                    TargetDate = progressRecord?.TargetDate,
                    ProgressRecordId = progressRecord?.Id
                });
            }

            ViewBag.NameSearch = nameSearch;
            ViewBag.CompanyId = companyId;
            return View(scheduleItems);
        }

        // POST: EmployeeOnboarding/SaveTargetDate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTargetDate(int employeeId, int stepId, DateOnly? targetDate)
        {
            var progress = await _context.EmployeeOnboardingProgresses
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.StepId == stepId);

            if (progress == null)
            {
                progress = new EmployeeOnboardingProgress
                {
                    EmployeeId = employeeId,
                    StepId = stepId,
                    IsCompleted = false,
                    TargetDate = targetDate
                };
                _context.EmployeeOnboardingProgresses.Add(progress);
            }
            else
            {
                progress.TargetDate = targetDate;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حفظ الموعد بنجاح";
            return RedirectToAction(nameof(Schedule));
        }
    }
}