using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRentWeb.Data;
using CarRentWeb.Models;

namespace CarRentWeb.Controllers
{
    public class PeriodicTaskInstanceStepsController : Controller
    {
        private readonly CarRentWebContext _context;

        public PeriodicTaskInstanceStepsController(CarRentWebContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? periodicTaskId)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            TempData.Keep();

            var allTasks = await _context.PeriodicTasks
                .Where(t => t.IsActive)
                .OrderBy(t => t.TaskOrder)
                .ToListAsync();

            ViewBag.PeriodicTasks = new SelectList(allTasks, "Id", "TaskName", periodicTaskId);
            ViewBag.SelectedTaskId = periodicTaskId;

            List<PeriodicTaskInstanceStep> steps = new();
            if (periodicTaskId.HasValue)
            {
                steps = await _context.PeriodicTaskInstanceSteps
                    .Where(s => s.PeriodicTaskId == periodicTaskId.Value)
                    .OrderBy(s => s.StepOrder)
                    .ThenBy(s => s.Id)
                    .ToListAsync();

                ViewBag.SelectedTask = allTasks.FirstOrDefault(t => t.Id == periodicTaskId.Value);
            }

            ViewBag.NextOrder = steps.Any() ? steps.Max(s => s.StepOrder) + 1 : 1;
            return View(steps);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int periodicTaskId, string stepName, string? description,
            int stepOrder, string? location, string? jeha)
        {
            if (!string.IsNullOrWhiteSpace(stepName))
            {
                _context.PeriodicTaskInstanceSteps.Add(new PeriodicTaskInstanceStep
                {
                    PeriodicTaskId = periodicTaskId,
                    StepName = stepName.Trim(),
                    Description = description?.Trim(),
                    StepOrder = stepOrder,
                    IsActive = true,
                    Location = location?.Trim(),
                    Jeha = jeha?.Trim()
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { periodicTaskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int periodicTaskId, string stepName, string? description,
            int stepOrder, string? location, string? jeha)
        {
            var step = await _context.PeriodicTaskInstanceSteps.FindAsync(id);
            if (step == null) return NotFound();

            step.StepName = stepName.Trim();
            step.Description = description?.Trim();
            step.StepOrder = stepOrder;
            step.Location = location?.Trim();
            step.Jeha = jeha?.Trim();
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { periodicTaskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, int periodicTaskId)
        {
            var step = await _context.PeriodicTaskInstanceSteps.FindAsync(id);
            if (step == null) return NotFound();

            step.IsActive = !step.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { periodicTaskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int periodicTaskId)
        {
            var step = await _context.PeriodicTaskInstanceSteps.FindAsync(id);
            if (step != null)
            {
                _context.PeriodicTaskInstanceSteps.Remove(step);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { periodicTaskId });
        }
    }
}
