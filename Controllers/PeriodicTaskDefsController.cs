using CarRentWeb.Data;
using CarRentWeb.Helpers;
using CarRentWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentWeb.Controllers;

public class PeriodicTaskDefsController : Controller
{
    private readonly CarRentWebContext _context;

    public PeriodicTaskDefsController(CarRentWebContext context)
    {
        _context = context;
    }

    // GET: تعريف المهام الدورية
    public async Task<IActionResult> Index()
    {
        var defs = await _context.PeriodicTaskDefs
            .Include(d => d.Steps.OrderBy(s => s.StepOrder))
            .OrderBy(d => d.SourceType).ThenBy(d => d.TaskName)
            .ToListAsync();

        ViewBag.EmployeeFields = PeriodicTaskHelper.EmployeeDateFields;
        ViewBag.CarFields = PeriodicTaskHelper.CarDateFields;
        return View(defs);
    }

    // GET: إضافة مهمة دورية
    public IActionResult Create()
    {
        ViewBag.EmployeeFields = PeriodicTaskHelper.EmployeeDateFields;
        ViewBag.CarFields = PeriodicTaskHelper.CarDateFields;
        return View(new PeriodicTaskDef { AlertDaysBefore = 30, IsActive = true });
    }

    // POST: إضافة مهمة دورية
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PeriodicTaskDef model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EmployeeFields = PeriodicTaskHelper.EmployeeDateFields;
            ViewBag.CarFields = PeriodicTaskHelper.CarDateFields;
            return View(model);
        }

        _context.PeriodicTaskDefs.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(EditSteps), new { id = model.Id });
    }

    // GET: تعديل المهمة وإدارة خطواتها
    public async Task<IActionResult> EditSteps(int id)
    {
        var def = await _context.PeriodicTaskDefs
            .Include(d => d.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(d => d.Id == id);

        if (def == null) return NotFound();

        ViewBag.EmployeeFields = PeriodicTaskHelper.EmployeeDateFields;
        ViewBag.CarFields = PeriodicTaskHelper.CarDateFields;
        return View(def);
    }

    // POST: تعديل بيانات المهمة
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSteps(PeriodicTaskDef model)
    {
        if (!ModelState.IsValid)
        {
            model.Steps = (await _context.PeriodicTaskSteps
                .Where(s => s.TaskDefId == model.Id)
                .OrderBy(s => s.StepOrder)
                .ToListAsync());
            ViewBag.EmployeeFields = PeriodicTaskHelper.EmployeeDateFields;
            ViewBag.CarFields = PeriodicTaskHelper.CarDateFields;
            return View(model);
        }

        _context.Update(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(EditSteps), new { id = model.Id });
    }

    // POST: إضافة خطوة
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStep(int taskDefId, string stepName, string? authority, string? location)
    {
        var maxOrder = await _context.PeriodicTaskSteps
            .Where(s => s.TaskDefId == taskDefId)
            .Select(s => (int?)s.StepOrder)
            .MaxAsync() ?? 0;

        var step = new PeriodicTaskStep
        {
            TaskDefId = taskDefId,
            StepOrder = maxOrder + 1,
            StepName = stepName,
            Authority = authority,
            Location = location
        };

        _context.PeriodicTaskSteps.Add(step);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(EditSteps), new { id = taskDefId });
    }

    // POST: حذف خطوة
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStep(int stepId, int taskDefId)
    {
        var step = await _context.PeriodicTaskSteps.FindAsync(stepId);
        if (step != null)
        {
            _context.PeriodicTaskSteps.Remove(step);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(EditSteps), new { id = taskDefId });
    }

    // POST: حذف مهمة دورية
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var def = await _context.PeriodicTaskDefs.FindAsync(id);
        if (def != null)
        {
            _context.PeriodicTaskDefs.Remove(def);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}