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
        public async Task<IActionResult> Index(int? searchCode, string? nameSearch, int? companyId)
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

            ViewBag.TotalSteps = totalSteps;
            ViewBag.ProgressData = progressData;
            ViewBag.SearchCode = searchCode;
            ViewBag.NameSearch = nameSearch;

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
    }
}
