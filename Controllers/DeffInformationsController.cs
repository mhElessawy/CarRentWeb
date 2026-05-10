
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRentWeb.Models;
using CarRentWeb.Data;

namespace CarRentWeb.Controllers
{
    public class DeffInformationsController : Controller
    {
        private readonly CarRentWebContext _context;

        public DeffInformationsController(CarRentWebContext context)
        {
            _context = context;
        }

        // GET: DeffInformations
        public async Task<IActionResult> Index()
        {
            var rahalWebContext = _context.DeffInformation.Include(d => d.DeffPayLate);
            return View(await rahalWebContext.ToListAsync());
        }

        // GET: DeffInformations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deffInformation = await _context.DeffInformation
                .Include(d => d.DeffPayLate)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deffInformation == null)
            {
                return NotFound();
            }

            return View(deffInformation);
        }



        // GET: DeffInformations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deffInformation = await _context.DeffInformation.FindAsync(id);
            if (deffInformation == null)
            {
                return NotFound();
            }
            ViewData["DebitPayLatId"] = new SelectList(_context.Deffs, "Id", "DeffName", deffInformation.DebitPayLatId);
            return View(deffInformation);
        }

        // POST: DeffInformations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DebitPayLateDay,DebitPayLatId,VpsRenewalDate,SiteUrl,SiteUsername,SitePassword,Domain,DomainRenewalDate,DomainUsername,DomainPassword,SslUrl,SslExpiry,SslUsername,SslPassword,MessageUrl,MessageRenewalDate,MessageUsername,MessagePassword")] DeffInformation deffInformation)
        {
            if (id != deffInformation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deffInformation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeffInformationExists(deffInformation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DebitPayLatId"] = new SelectList(_context.Deffs, "Id", "DeffName", deffInformation.DebitPayLatId);
            return View(deffInformation);
        }

        // GET: DeffInformations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deffInformation = await _context.DeffInformation
                .Include(d => d.DeffPayLate)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deffInformation == null)
            {
                return NotFound();
            }

            return View(deffInformation);
        }

        // POST: DeffInformations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deffInformation = await _context.DeffInformation.FindAsync(id);
            if (deffInformation != null)
            {
                _context.DeffInformation.Remove(deffInformation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeffInformationExists(int id)
        {
            return _context.DeffInformation.Any(e => e.Id == id);
        }
    }
}
