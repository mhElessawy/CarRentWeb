using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentWeb.Data;
using CarRentWeb.Models;

namespace CarRentWeb.Controllers
{
    public class DriverOnboardingStepsController : Controller
    {
        private readonly CarRentWebContext _context;

        public DriverOnboardingStepsController(CarRentWebContext context)
        {
            _context = context;
        }

        // GET: DriverOnboardingSteps
        public async Task<IActionResult> Index()
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            TempData.Keep();

            var steps = await _context.DriverOnboardingSteps
                .OrderBy(s => s.StepOrder)
                .ThenBy(s => s.Id)
                .ToListAsync();

            return View(steps);
        }

        // POST: DriverOnboardingSteps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string stepName, string? description, int stepOrder)
        {
            if (!string.IsNullOrWhiteSpace(stepName))
            {
                var step = new DriverOnboardingStep
                {
                    StepName = stepName.Trim(),
                    Description = description?.Trim(),
                    StepOrder = stepOrder,
                    IsActive = true
                };
                _context.DriverOnboardingSteps.Add(step);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: DriverOnboardingSteps/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string stepName, string? description, int stepOrder)
        {
            var step = await _context.DriverOnboardingSteps.FindAsync(id);
            if (step == null) return NotFound();

            step.StepName = stepName.Trim();
            step.Description = description?.Trim();
            step.StepOrder = stepOrder;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: DriverOnboardingSteps/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var step = await _context.DriverOnboardingSteps.FindAsync(id);
            if (step == null) return NotFound();

            step.IsActive = !step.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: DriverOnboardingSteps/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var step = await _context.DriverOnboardingSteps.FindAsync(id);
            if (step != null)
            {
                _context.DriverOnboardingSteps.Remove(step);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}