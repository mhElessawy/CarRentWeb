using CarRentWeb.Data;
using CarRentWeb.Models;
using CarRentWeb.Models.MyModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentWeb.Controllers;

public class SteppedAdminTasksController : Controller
{
    private readonly CarRentWebContext _context;

    public SteppedAdminTasksController(CarRentWebContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? status, string? search, string? assignedTo)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var query = _context.SteppedAdminTasks
            .Include(t => t.Steps)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.TaskTitle.Contains(search) ||
                                     (t.Notes != null && t.Notes.Contains(search)));

        if (!string.IsNullOrEmpty(assignedTo))
            query = query.Where(t => t.AssignedTo == assignedTo);

        var all = await query.OrderBy(t => t.IsCompleted)
                             .ThenBy(t => t.CreatedDate)
                             .ToListAsync();

        var filtered = status switch
        {
            "completed" => all.Where(t => t.IsCompleted).ToList(),
            "pending" => all.Where(t => !t.IsCompleted).ToList(),
            _ => all
        };

        ViewBag.Status = status ?? "";
        ViewBag.Search = search ?? "";
        ViewBag.AssignedTo = assignedTo ?? "";
        ViewBag.Today = today;
        ViewBag.Users = await _context.PasswordData
            .Where(u => u.DeleteFlag != 1)
            .OrderBy(u => u.UserFullName)
            .Select(u => new { u.UserName, u.UserFullName })
            .ToListAsync();

        return View(filtered);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Users = await _context.PasswordData
            .Where(u => u.DeleteFlag != 1)
            .OrderBy(u => u.UserFullName)
            .Select(u => new { u.UserName, u.UserFullName })
            .ToListAsync();

        return View(new SteppedAdminTaskViewModel
        {
            Steps = new List<SteppedAdminTaskViewModel.StepInput>
            {
                new() { StepOrder = 1, TargetDate = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd") }
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SteppedAdminTaskViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.TaskTitle) || vm.Steps == null || !vm.Steps.Any())
        {
            TempData["Error"] = "يرجى إدخال عنوان المهمة وخطوة واحدة على الأقل";
            return await ReturnCreateView(vm);
        }

        var task = new SteppedAdminTask
        {
            TaskTitle = vm.TaskTitle,
            AssignedTo = vm.AssignedTo,
            Notes = vm.Notes,
            CreatedDate = DateTime.Now,
            CreatedBy = HttpContext.Session.GetString("Username"),
            IsCompleted = false
        };

        for (int i = 0; i < vm.Steps.Count; i++)
        {
            var s = vm.Steps[i];
            if (string.IsNullOrWhiteSpace(s.StepName)) continue;
            task.Steps.Add(new SteppedAdminTaskStep
            {
                StepOrder = i + 1,
                StepName = s.StepName,
                TargetDate = DateOnly.Parse(s.TargetDate),
                Notes = s.Notes
            });
        }

        if (!task.Steps.Any())
        {
            TempData["Error"] = "يرجى إضافة خطوة واحدة على الأقل";
            return await ReturnCreateView(vm);
        }

        _context.SteppedAdminTasks.Add(task);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم إضافة المهمة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var task = await _context.SteppedAdminTasks
            .Include(t => t.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return NotFound();

        ViewBag.Today = DateOnly.FromDateTime(DateTime.Today);
        return View(task);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var task = await _context.SteppedAdminTasks
            .Include(t => t.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return NotFound();

        ViewBag.Users = await _context.PasswordData
            .Where(u => u.DeleteFlag != 1)
            .OrderBy(u => u.UserFullName)
            .Select(u => new { u.UserName, u.UserFullName })
            .ToListAsync();

        var vm = new SteppedAdminTaskViewModel
        {
            Id = task.Id,
            TaskTitle = task.TaskTitle,
            AssignedTo = task.AssignedTo,
            Notes = task.Notes,
            Steps = task.Steps.Select(s => new SteppedAdminTaskViewModel.StepInput
            {
                Id = s.Id,
                StepName = s.StepName,
                TargetDate = s.TargetDate.ToString("yyyy-MM-dd"),
                Notes = s.Notes,
                StepOrder = s.StepOrder
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SteppedAdminTaskViewModel vm)
    {
        var task = await _context.SteppedAdminTasks
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return NotFound();

        task.TaskTitle = vm.TaskTitle;
        task.AssignedTo = vm.AssignedTo;
        task.Notes = vm.Notes;

        // Remove old steps
        _context.SteppedAdminTaskSteps.RemoveRange(task.Steps);
        task.Steps.Clear();

        for (int i = 0; i < vm.Steps.Count; i++)
        {
            var s = vm.Steps[i];
            if (string.IsNullOrWhiteSpace(s.StepName)) continue;
            task.Steps.Add(new SteppedAdminTaskStep
            {
                StepOrder = i + 1,
                StepName = s.StepName,
                TargetDate = DateOnly.Parse(s.TargetDate),
                Notes = s.Notes
            });
        }

        // Auto-complete task if all steps done
        if (task.Steps.Any() && task.Steps.All(s => s.IsCompleted))
        {
            task.IsCompleted = true;
            task.CompletedDate ??= DateOnly.FromDateTime(DateTime.Today);
        }
        else
        {
            task.IsCompleted = false;
            task.CompletedDate = null;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "تم تحديث المهمة بنجاح";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStep(int stepId, int taskId)
    {
        var step = await _context.SteppedAdminTaskSteps.FindAsync(stepId);
        if (step == null) return NotFound();

        step.IsCompleted = !step.IsCompleted;
        step.CompletedDate = step.IsCompleted ? DateOnly.FromDateTime(DateTime.Today) : null;

        // Check if all steps of parent task are done
        var task = await _context.SteppedAdminTasks
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == step.TaskId);

        if (task != null)
        {
            task.IsCompleted = task.Steps.All(s => s.IsCompleted);
            task.CompletedDate = task.IsCompleted ? DateOnly.FromDateTime(DateTime.Today) : null;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = step.TaskId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.SteppedAdminTasks.FindAsync(id);
        if (task != null)
        {
            _context.SteppedAdminTasks.Remove(task);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف المهمة";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnCreateView(SteppedAdminTaskViewModel vm)
    {
        ViewBag.Users = await _context.PasswordData
            .Where(u => u.DeleteFlag != 1)
            .OrderBy(u => u.UserFullName)
            .Select(u => new { u.UserName, u.UserFullName })
            .ToListAsync();
        return View("Create", vm);
    }
}
