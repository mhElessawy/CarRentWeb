using CarRentWeb.Data;
using CarRentWeb.Helpers;
using CarRentWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentWeb.Controllers;

public class TasksReportController : Controller
{
    private readonly CarRentWebContext _context;

    public TasksReportController(CarRentWebContext context)
    {
        _context = context;
    }

    // GET: تقرير شامل لجميع المهام
    public async Task<IActionResult> Index(
        string? taskType,   // "Foundation" | "Periodic" | "Admin" | ""
        string? status,     // "completed" | "overdue" | "pending" | ""
        string? search,
        DateOnly? dateFrom,
        DateOnly? dateTo)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        ViewBag.TaskType = taskType ?? "";
        ViewBag.Status = status ?? "";
        ViewBag.Search = search ?? "";
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd") ?? "";
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd") ?? "";
        ViewBag.Today = today;

        var items = new List<UnifiedTaskReportItem>();

        // ── 1. مهام تأسيسية ───────────────────────────────────────────────
        if (string.IsNullOrEmpty(taskType) || taskType == "Foundation")
        {
            var onboarding = await _context.EmployeeOnboardingProgresses
                .Include(p => p.Employee)
                .Include(p => p.Step)
                .ToListAsync();

            foreach (var p in onboarding)
            {
                string entityName = p.Employee?.FullNameAr
                    ?? $"{p.Employee?.FirstNameAr} {p.Employee?.LastNameAr}".Trim();
                string taskName = p.Step?.StepName ?? "";

                if (!string.IsNullOrEmpty(search) &&
                    !entityName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                    !taskName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    continue;

                int? delay = p.TargetDate.HasValue
                    ? today.DayNumber - p.TargetDate.Value.DayNumber
                    : null;

                items.Add(new UnifiedTaskReportItem
                {
                    TaskType = "Foundation",
                    TaskTypeName = "تأسيسية",
                    TaskName = taskName,
                    EntityName = entityName,
                    TargetDate = p.TargetDate,
                    CompletedDate = p.CompletedDate,
                    IsCompleted = p.IsCompleted,
                    DelayDays = delay
                });
            }
        }

        // ── 2. مهام دورية ────────────────────────────────────────────────
        if (string.IsNullOrEmpty(taskType) || taskType == "Periodic")
        {
            var instances = await _context.PeriodicTaskInstances
                .Include(i => i.TaskDef)
                .ToListAsync();

            var employees = await _context.EmployeeInfos
                .Where(e => e.DeleteFlag == 0).ToListAsync();
            var cars = await _context.CarInfos
                .Where(c => c.DeleteFlag == 0).ToListAsync();

            foreach (var i in instances)
            {
                string entityName = i.TaskDef?.SourceType == "Car"
                    ? $"سيارة رقم {cars.FirstOrDefault(c => c.Id == i.SourceId)?.CarNo ?? i.SourceId.ToString()}"
                    : employees.FirstOrDefault(e => e.Id == i.SourceId)?.FullNameAr ?? $"موظف #{i.SourceId}";
                string taskName = i.TaskDef?.TaskName ?? "";

                if (!string.IsNullOrEmpty(search) &&
                    !entityName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                    !taskName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    continue;

                int? delay = i.TargetDate.HasValue
                    ? today.DayNumber - i.TargetDate.Value.DayNumber
                    : null;

                items.Add(new UnifiedTaskReportItem
                {
                    TaskType = "Periodic",
                    TaskTypeName = "دورية",
                    TaskName = taskName,
                    EntityName = entityName,
                    TargetDate = i.TargetDate,
                    CompletedDate = null,
                    IsCompleted = i.IsCompleted,
                    DelayDays = delay
                });
            }
        }

        // ── 3. مهام إدارية ───────────────────────────────────────────────
        if (string.IsNullOrEmpty(taskType) || taskType == "Admin")
        {
            var adminTasks = await _context.AdminTasks.ToListAsync();

            foreach (var t in adminTasks)
            {
                if (!string.IsNullOrEmpty(search) &&
                    !t.TaskTitle.Contains(search, StringComparison.OrdinalIgnoreCase))
                    continue;

                int delay = today.DayNumber - t.TargetDate.DayNumber;

                items.Add(new UnifiedTaskReportItem
                {
                    TaskType = "Admin",
                    TaskTypeName = "إدارية",
                    TaskName = t.TaskTitle,
                    EntityName = t.CreatedBy ?? "",
                    TargetDate = t.TargetDate,
                    CompletedDate = t.CompletedDate,
                    IsCompleted = t.IsCompleted,
                    DelayDays = delay
                });
            }
        }

        // ── فلتر التاريخ ──────────────────────────────────────────────────
        if (dateFrom.HasValue)
            items = items.Where(x => x.TargetDate >= dateFrom.Value).ToList();
        if (dateTo.HasValue)
            items = items.Where(x => x.TargetDate <= dateTo.Value).ToList();

        // ── فلتر الحالة ──────────────────────────────────────────────────
        items = (status switch
        {
            "completed" => items.Where(x => x.IsCompleted),
            "overdue" => items.Where(x => !x.IsCompleted && x.DelayDays.HasValue && x.DelayDays > 0),
            "pending" => items.Where(x => !x.IsCompleted),
            _ => items.AsEnumerable()
        }).ToList();

        // ── ترتيب ────────────────────────────────────────────────────────
        var sorted = items
            .OrderBy(x => x.IsCompleted)
            .ThenByDescending(x => x.DelayDays)
            .ThenBy(x => x.TargetDate ?? DateOnly.MaxValue)
            .ToList();

        return View(sorted);
    }
}