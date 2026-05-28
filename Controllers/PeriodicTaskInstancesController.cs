using CarRentWeb.Data;
using CarRentWeb.Helpers;
using CarRentWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentWeb.Controllers;

public class PeriodicTaskInstancesController : Controller
{
    private readonly CarRentWebContext _context;

    public PeriodicTaskInstancesController(CarRentWebContext context)
    {
        _context = context;
    }

    // GET: المهام النشطة
    public async Task<IActionResult> Index(int? taskDefId, string? sourceType, string? status)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var allDefs = await _context.PeriodicTaskDefs
            .Where(d => d.IsActive)
            .Include(d => d.Steps)
            .OrderBy(d => d.TaskName)
            .ToListAsync();

        var defs = allDefs;
        if (taskDefId.HasValue)
            defs = defs.Where(d => d.Id == taskDefId.Value).ToList();
        if (!string.IsNullOrEmpty(sourceType))
            defs = defs.Where(d => d.SourceType == sourceType).ToList();

        var activeItems = new List<ActiveTaskViewModel>();

        foreach (var def in defs)
        {
            if (def.SourceType == "Employee")
            {
                var employees = await _context.EmployeeInfos
                    .Where(e => e.DeleteFlag == 0)
                    .ToListAsync();

                foreach (var emp in employees)
                {
                    var dueDate = PeriodicTaskHelper.GetEmployeeDate(emp, def.DateFieldName);
                    if (dueDate == null) continue;

                    var daysLeft = dueDate.Value.DayNumber - today.DayNumber;
                    if (daysLeft > def.AlertDaysBefore) continue;

                    var instance = await GetOrCreateInstance(def, emp.Id, dueDate.Value);
                    activeItems.Add(new ActiveTaskViewModel
                    {
                        Instance = instance,
                        Def = def,
                        EntityName = emp.FullNameAr ?? $"{emp.FirstNameAr} {emp.LastNameAr}",
                        DueDate = dueDate.Value,
                        DaysLeft = daysLeft,
                        FieldLabel = PeriodicTaskHelper.GetFieldLabel(def.SourceType, def.DateFieldName)
                    });
                }
            }
            else if (def.SourceType == "Car")
            {
                var cars = await _context.CarInfos
                    .Where(c => c.DeleteFlag == 0)
                    .ToListAsync();

                foreach (var car in cars)
                {
                    var dueDate = PeriodicTaskHelper.GetCarDate(car, def.DateFieldName);
                    if (dueDate == null) continue;

                    var daysLeft = dueDate.Value.DayNumber - today.DayNumber;
                    if (daysLeft > def.AlertDaysBefore) continue;

                    var instance = await GetOrCreateInstance(def, car.Id, dueDate.Value);
                    activeItems.Add(new ActiveTaskViewModel
                    {
                        Instance = instance,
                        Def = def,
                        EntityName = $"سيارة رقم {car.CarNo}",
                        DueDate = dueDate.Value,
                        DaysLeft = daysLeft,
                        FieldLabel = PeriodicTaskHelper.GetFieldLabel(def.SourceType, def.DateFieldName)
                    });
                }
            }
        }

        // فلتر الحالة
        if (status == "pending")
            activeItems = activeItems.Where(x => !x.Instance.IsCompleted).ToList();
        else if (status == "completed")
            activeItems = activeItems.Where(x => x.Instance.IsCompleted).ToList();

        var sorted = activeItems
            .OrderBy(x => x.Instance.IsCompleted)
            .ThenBy(x => x.DaysLeft)
            .ToList();

        ViewBag.AllDefs = allDefs;
        ViewBag.SelectedTaskDefId = taskDefId;
        ViewBag.SelectedSourceType = sourceType;
        ViewBag.SelectedStatus = status;

        return View(sorted);
    }

    // GET: تفاصيل مهمة ومتابعة خطواتها
    public async Task<IActionResult> Details(int id)
    {
        var instance = await _context.PeriodicTaskInstances
            .Include(i => i.TaskDef)
                .ThenInclude(d => d.Steps.OrderBy(s => s.StepOrder))
            .Include(i => i.StepStatuses)
                .ThenInclude(ss => ss.Step)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instance == null) return NotFound();

        // إنشاء حالة لكل خطوة إذا لم تكن موجودة
        foreach (var step in instance.TaskDef.Steps)
        {
            if (!instance.StepStatuses.Any(ss => ss.StepId == step.Id))
            {
                var newStatus = new PeriodicTaskInstanceStep
                {
                    InstanceId = instance.Id,
                    StepId = step.Id,
                    IsCompleted = false
                };
                _context.PeriodicTaskInstanceSteps.Add(newStatus);
            }
        }
        await _context.SaveChangesAsync();

        // إعادة التحميل بعد الحفظ
        instance = await _context.PeriodicTaskInstances
            .Include(i => i.TaskDef)
                .ThenInclude(d => d.Steps.OrderBy(s => s.StepOrder))
            .Include(i => i.StepStatuses)
                .ThenInclude(ss => ss.Step)
            .FirstOrDefaultAsync(i => i.Id == id);

        var today = DateOnly.FromDateTime(DateTime.Today);
        string entityName;
        if (instance!.TaskDef.SourceType == "Employee")
        {
            var emp = await _context.EmployeeInfos.FindAsync(instance.SourceId);
            entityName = emp?.FullNameAr ?? $"موظف #{instance.SourceId}";
        }
        else
        {
            var car = await _context.CarInfos.FindAsync(instance.SourceId);
            entityName = $"سيارة رقم {car?.CarNo ?? instance.SourceId.ToString()}";
        }

        ViewBag.EntityName = entityName;
        ViewBag.FieldLabel = PeriodicTaskHelper.GetFieldLabel(instance.TaskDef.SourceType, instance.TaskDef.DateFieldName);
        ViewBag.DaysLeft = instance.DueDate.DayNumber - today.DayNumber;
        return View(instance);
    }

    // POST: تحديث حالة خطوة
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStep(int stepStatusId, int instanceId)
    {
        var status = await _context.PeriodicTaskInstanceSteps.FindAsync(stepStatusId);
        if (status != null)
        {
            status.IsCompleted = !status.IsCompleted;
            status.CompletedDate = status.IsCompleted
                ? DateOnly.FromDateTime(DateTime.Today)
                : null;
            await _context.SaveChangesAsync();

            // تحديث حالة المهمة الكاملة إذا اكتملت جميع الخطوات
            var instance = await _context.PeriodicTaskInstances
                .Include(i => i.StepStatuses)
                .FirstOrDefaultAsync(i => i.Id == instanceId);
            if (instance != null)
            {
                instance.IsCompleted = instance.StepStatuses.All(s => s.IsCompleted);
                await _context.SaveChangesAsync();
            }
        }
        return RedirectToAction(nameof(Details), new { id = instanceId });
    }

    // POST: تعليم المهمة كمكتملة / إعادة فتحها
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComplete(int id)
    {
        var instance = await _context.PeriodicTaskInstances
            .Include(i => i.StepStatuses)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instance != null)
        {
            instance.IsCompleted = !instance.IsCompleted;
            if (instance.IsCompleted)
            {
                foreach (var s in instance.StepStatuses)
                {
                    s.IsCompleted = true;
                    s.CompletedDate ??= DateOnly.FromDateTime(DateTime.Today);
                }
            }
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: جدول مهام المندوب للمهام الدورية
    public async Task<IActionResult> Schedule(int? taskDefId, string? sourceType, string? nameSearch,
        DateOnly? dateFrom, DateOnly? dateTo)
    {
        TempData["Username"] = HttpContext.Session.GetString("Username");
        TempData.Keep();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var allDefs = await _context.PeriodicTaskDefs
            .Where(d => d.IsActive)
            .OrderBy(d => d.TaskName)
            .ToListAsync();

        ViewBag.AllDefs = allDefs;
        ViewBag.SelectedTaskDefId = taskDefId;
        ViewBag.SelectedSourceType = sourceType ?? "";
        ViewBag.NameSearch = nameSearch;
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

        var query = _context.PeriodicTaskInstances
            .Include(i => i.TaskDef)
            .Where(i => !i.IsCompleted);

        if (taskDefId.HasValue)
            query = query.Where(i => i.TaskDefId == taskDefId.Value);
        if (!string.IsNullOrEmpty(sourceType))
            query = query.Where(i => i.TaskDef!.SourceType == sourceType);
        if (dateFrom.HasValue)
            query = query.Where(i => i.TargetDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(i => i.TargetDate <= dateTo.Value);

        var instances = await query.OrderBy(i => i.TargetDate ?? DateOnly.MaxValue)
            .ThenBy(i => i.DueDate)
            .ToListAsync();

        // بناء قائمة الجدول مع أسماء الكيانات
        var employees = await _context.EmployeeInfos.Where(e => e.DeleteFlag == 0).ToListAsync();
        var cars = await _context.CarInfos.Where(c => c.DeleteFlag == 0).ToListAsync();

        var scheduleItems = instances.Select(i =>
        {
            string entityName = i.TaskDef?.SourceType == "Car"
                ? $"سيارة رقم {cars.FirstOrDefault(c => c.Id == i.SourceId)?.CarNo ?? i.SourceId.ToString()}"
                : employees.FirstOrDefault(e => e.Id == i.SourceId)?.FullNameAr ?? $"موظف #{i.SourceId}";

            if (!string.IsNullOrEmpty(nameSearch) &&
                !entityName.Contains(nameSearch, StringComparison.OrdinalIgnoreCase))
                return null;

            return new PeriodicScheduleItem
            {
                InstanceId = i.Id,
                TaskName = i.TaskDef?.TaskName ?? "",
                SourceType = i.TaskDef?.SourceType ?? "",
                EntityName = entityName,
                DueDate = i.DueDate,
                TargetDate = i.TargetDate,
                DaysLeft = i.DueDate.DayNumber - today.DayNumber,
                FieldLabel = i.TaskDef != null
                    ? PeriodicTaskHelper.GetFieldLabel(i.TaskDef.SourceType, i.TaskDef.DateFieldName)
                    : ""
            };
        }).Where(x => x != null).Cast<PeriodicScheduleItem>().ToList();

        return View(scheduleItems);
    }

    // POST: حفظ تاريخ التنفيذ
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTargetDate(int instanceId, DateOnly? targetDate,
        int? taskDefId, string? sourceType, string? nameSearch, string? dateFrom, string? dateTo)
    {
        var instance = await _context.PeriodicTaskInstances.FindAsync(instanceId);
        if (instance != null)
        {
            instance.TargetDate = targetDate;
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حفظ تاريخ التنفيذ بنجاح";
        }

        DateOnly? parsedFrom = DateOnly.TryParse(dateFrom, out var df) ? df : null;
        DateOnly? parsedTo = DateOnly.TryParse(dateTo, out var dt) ? dt : null;

        return RedirectToAction(nameof(Schedule), new
        {
            taskDefId,
            sourceType,
            nameSearch,
            dateFrom = parsedFrom?.ToString("yyyy-MM-dd"),
            dateTo = parsedTo?.ToString("yyyy-MM-dd")
        });
    }

    private async Task<PeriodicTaskInstance> GetOrCreateInstance(PeriodicTaskDef def, int sourceId, DateOnly dueDate)
    {
        var existing = await _context.PeriodicTaskInstances
            .Include(i => i.StepStatuses)
            .FirstOrDefaultAsync(i => i.TaskDefId == def.Id && i.SourceId == sourceId);

        if (existing != null) return existing;

        var instance = new PeriodicTaskInstance
        {
            TaskDefId = def.Id,
            SourceId = sourceId,
            DueDate = dueDate,
            CreatedDate = DateTime.Now,
            IsCompleted = false
        };
        _context.PeriodicTaskInstances.Add(instance);
        await _context.SaveChangesAsync();
        return instance;
    }
}