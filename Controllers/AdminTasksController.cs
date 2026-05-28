using CarRentWeb.Data;
using CarRentWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentWeb.Controllers;

public class AdminTasksController : Controller
{
    private readonly CarRentWebContext _context;

    public AdminTasksController(CarRentWebContext context)
    {
        _context = context;
    }

    // GET: قائمة المهام الإدارية
    public async Task<IActionResult> Index(string? status, string? search,
        DateOnly? dateFrom, DateOnly? dateTo)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var query = _context.AdminTasks.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.TaskTitle.Contains(search) ||
                                     (t.Notes != null && t.Notes.Contains(search)));
        if (dateFrom.HasValue)
            query = query.Where(t => t.TargetDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(t => t.TargetDate <= dateTo.Value);

        var all = await query.OrderBy(t => t.IsCompleted)
                             .ThenBy(t => t.TargetDate)
                             .ToListAsync();

        var filtered = status switch
        {
            "completed" => all.Where(t => t.IsCompleted).ToList(),
            "overdue" => all.Where(t => !t.IsCompleted && t.TargetDate < today).ToList(),
            "pending" => all.Where(t => !t.IsCompleted).ToList(),
            _ => all
        };

        ViewBag.Status = status ?? "";
        ViewBag.Search = search ?? "";
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd") ?? "";
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd") ?? "";
        ViewBag.Today = today;

        return View(filtered);
    }

    // GET: إضافة مهمة جديدة
    public IActionResult Create()
    {
        return View(new AdminTask
        {
            TargetDate = DateOnly.FromDateTime(DateTime.Today),
            CreatedDate = DateTime.Now
        });
    }

    // POST: حفظ مهمة جديدة
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminTask task)
    {
        if (!ModelState.IsValid) return View(task);

        task.CreatedDate = DateTime.Now;
        task.CreatedBy = HttpContext.Session.GetString("Username");
        task.IsCompleted = false;

        _context.AdminTasks.Add(task);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم إضافة المهمة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    // GET: تعديل مهمة
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _context.AdminTasks.FindAsync(id);
        if (task == null) return NotFound();
        return View(task);
    }

    // POST: حفظ التعديل
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminTask task)
    {
        if (id != task.Id) return NotFound();
        if (!ModelState.IsValid) return View(task);

        _context.Update(task);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم تحديث المهمة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    // POST: تغيير حالة الإتمام
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComplete(int id,
        string? status, string? search, string? dateFrom, string? dateTo)
    {
        var task = await _context.AdminTasks.FindAsync(id);
        if (task != null)
        {
            task.IsCompleted = !task.IsCompleted;
            task.CompletedDate = task.IsCompleted
                ? DateOnly.FromDateTime(DateTime.Today)
                : null;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index), new { status, search, dateFrom, dateTo });
    }

    // POST: حذف مهمة
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.AdminTasks.FindAsync(id);
        if (task != null)
        {
            _context.AdminTasks.Remove(task);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف المهمة";
        }
        return RedirectToAction(nameof(Index));
    }
}