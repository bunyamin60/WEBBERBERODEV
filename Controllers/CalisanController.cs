using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBERBERODEV.DATA;
using WEBBERBERODEV.Models;

namespace WEBBERBERODEV.Controllers
{
    [Authorize(Roles = UserRoles.Role_Admin)] // Sadece Admin erişebilir
    public class CalisanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalisanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Calisan
        public async Task<IActionResult> Index()
        {
            var calisanlar = _context.Calisanlar.Include(c => c.Salon);
            return View(await calisanlar.ToListAsync());
        }

        // GET: Calisan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calisan = await _context.Calisanlar
                .Include(c => c.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calisan == null)
            {
                return NotFound();
            }

            return View(calisan);
        }

        // GET: Calisan/Create
        public IActionResult Create()
        {
            // Salon seçimine gerek yok, çünkü sadece bir salon var
            return View();
        }

        // POST: Calisan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ad,Soyad,Uzmanlik,AktifMi")] Calisan calisan)
        {
            calisan.SalonId = 1; // Otomatik olarak SalonId'yi 1 olarak ayarla
            if (ModelState.IsValid)
            {
                
                _context.Add(calisan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(calisan);
        }

        // GET: Calisan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calisan = await _context.Calisanlar.FindAsync(id);
            if (calisan == null)
            {
                return NotFound();
            }
            return View(calisan);
        }

        // POST: Calisan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Soyad,Uzmanlik,AktifMi")] Calisan calisan)
        {
            if (id != calisan.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    calisan.SalonId = 1; // Otomatik olarak SalonId'yi 1 olarak ayarla
                    _context.Update(calisan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CalisanExists(calisan.Id))
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
            return View(calisan);
        }

        // GET: Calisan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calisan = await _context.Calisanlar
                .Include(c => c.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calisan == null)
            {
                return NotFound();
            }

            return View(calisan);
        }

        // POST: Calisan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var calisan = await _context.Calisanlar.FindAsync(id);
            _context.Calisanlar.Remove(calisan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CalisanExists(int id)
        {
            return _context.Calisanlar.Any(e => e.Id == id);
        }
    }
}
