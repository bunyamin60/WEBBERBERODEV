using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBBERBERODEV.DATA;
using WEBBERBERODEV.Models;
using System.Threading.Tasks;
using System.Linq;

namespace WEBBERBERODEV.Controllers
{
    [Authorize(Roles = UserRoles.Role_Admin)]
    public class CalisanHizmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalisanHizmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CalisanHizmet
        public async Task<IActionResult> Index()
        {
            var calisanHizmetler = _context.CalisanHizmetler
                .Include(ch => ch.Calisan)
                .Include(ch => ch.Hizmet);
            return View(await calisanHizmetler.ToListAsync());
        }

        // GET: CalisanHizmet/Create
        public IActionResult Create()
        {
            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad");
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad");
            return View();
        }

        // POST: CalisanHizmet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CalisanId,HizmetId")] CalisanHizmet calisanHizmet)
        {
            if (ModelState.IsValid)
            {
                _context.CalisanHizmetler.Add(calisanHizmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", calisanHizmet.CalisanId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", calisanHizmet.HizmetId);
            return View(calisanHizmet);
        }

        // GET: CalisanHizmet/Delete/5/3 (CalisanId/HizmetId)
        public async Task<IActionResult> Delete(int? calisanId, int? hizmetId)
        {
            if (calisanId == null || hizmetId == null)
            {
                return NotFound();
            }

            var calisanHizmet = await _context.CalisanHizmetler
                .Include(ch => ch.Calisan)
                .Include(ch => ch.Hizmet)
                .FirstOrDefaultAsync(m => m.CalisanId == calisanId && m.HizmetId == hizmetId);
            if (calisanHizmet == null)
            {
                return NotFound();
            }

            return View(calisanHizmet);
        }

        // POST: CalisanHizmet/Delete/5/3
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int calisanId, int hizmetId)
        {
            var calisanHizmet = await _context.CalisanHizmetler.FindAsync(calisanId, hizmetId);
            if (calisanHizmet != null)
            {
                _context.CalisanHizmetler.Remove(calisanHizmet);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
