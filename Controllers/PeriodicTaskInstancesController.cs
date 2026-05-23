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

    // GET: المهام النشطة — تُولَّد تلقائياً من الموظفين والسيارات
    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var defs = await _context.PeriodicTaskDefs
            .Where(d => d.IsActive)
            .Include(d => d.Steps)
            .ToListAsync();

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

        var sorted = activeItems
            .OrderBy(x => x.Instance.IsCompleted)
            .ThenBy(x => x.DaysLeft)
            .ToList();

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

public class ActiveTaskViewModel
{
    public PeriodicTaskInstance Instance { get; set; } = null!;
    public PeriodicTaskDef Def { get; set; } = null!;
    public string EntityName { get; set; } = "";
    public DateOnly DueDate { get; set; }
    public int DaysLeft { get; set; }
    public string FieldLabel { get; set; } = "";
}
