using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentWeb.Data;
using CarRentWeb.Models;

namespace CarRentWeb.Controllers
{
    public class PeriodicTasksController : Controller
    {
        private readonly CarRentWebContext _context;

        public PeriodicTasksController(CarRentWebContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            TempData.Keep();

            var tasks = await _context.PeriodicTasks
                .OrderBy(t => t.TaskOrder)
                .ThenBy(t => t.Id)
                .ToListAsync();

            return View(tasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string taskName, string? description, int taskOrder,
            string? location, string? jeha, string? frequency)
        {
            if (!string.IsNullOrWhiteSpace(taskName))
            {
                var task = new PeriodicTask
                {
                    TaskName = taskName.Trim(),
                    Description = description?.Trim(),
                    TaskOrder = taskOrder,
                    IsActive = true,
                    Location = location?.Trim(),
                    Jeha = jeha?.Trim(),
                    Frequency = frequency?.Trim()
                };
                _context.PeriodicTasks.Add(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string taskName, string? description, int taskOrder,
            string? location, string? jeha, string? frequency)
        {
            var task = await _context.PeriodicTasks.FindAsync(id);
            if (task == null) return NotFound();

            task.TaskName = taskName.Trim();
            task.Description = description?.Trim();
            task.TaskOrder = taskOrder;
            task.Location = location?.Trim();
            task.Jeha = jeha?.Trim();
            task.Frequency = frequency?.Trim();
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var task = await _context.PeriodicTasks.FindAsync(id);
            if (task == null) return NotFound();

            task.IsActive = !task.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.PeriodicTasks.FindAsync(id);
            if (task != null)
            {
                _context.PeriodicTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}