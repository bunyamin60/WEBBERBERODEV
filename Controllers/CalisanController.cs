using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBERBERODEV.DATA;
using WEBBERBERODEV.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WEBBERBERODEV.Controllers
{
    [Authorize] // Sadece kimlik doğrulaması yapılmış kullanıcılar erişebilir
    public class CalisanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CalisanController(ApplicationDbContext context,
                                 UserManager<ApplicationUser> userManager,
                                 RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Calisan
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> Index()
        {
            var calisanlar = await _context.Calisanlar
                .Include(c => c.ApplicationUser)
                .ToListAsync();
            return View(calisanlar);
        }

        // GET: Calisan/Create
        [Authorize(Roles = UserRoles.Role_Admin)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Calisan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> Create(CalisanCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1) Önce Identity’de user oluştur
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Ad = model.Ad,
                Soyad = model.Soyad,
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(user, model.Password);
            if (!createUserResult.Succeeded)
            {
                // Hataları modelState’e ekleyip formu göster
                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // 2) User'a "Calisan" rolü ekle (eğer yoksa önce rolü oluştur)
            if (!await _roleManager.RoleExistsAsync(UserRoles.Role_Calisan))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Role_Calisan));
            }
            await _userManager.AddToRoleAsync(user, UserRoles.Role_Calisan);

            // 3) Şimdi Calisan tablosuna ekle
            var calisan = new Calisan
            {
                Ad = model.Ad,
                Soyad = model.Soyad,
                Uzmanlik = model.Uzmanlik,
                AktifMi = model.AktifMi,
                ApplicationUserId = user.Id // kimlik
            };

            _context.Calisanlar.Add(calisan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Calisan/Edit/5
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> Edit(int id)
        {
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
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> Edit(int id, Calisan calisan)
        {
            if (id != calisan.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(calisan);
            }

            try
            {
                // Dikkat: Identity tarafındaki (Email, Password) güncellemiyoruz.
                // Sadece Calisan tablosundaki Ad, Soyad, Uzmanlık vb. güncellenir.
                _context.Update(calisan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CalisanExists(calisan.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Calisan/Details/5
        [Authorize] // Her kimlik doğrulaması yapılmış kullanıcı erişebilir
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var calisan = await _context.Calisanlar
                .Include(c => c.CalisanCalismaSaatleri)
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calisan == null)
                return NotFound();

            return View(calisan);
        }

        // GET: Calisan/Delete/5
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var calisan = await _context.Calisanlar.FindAsync(id);
            if (calisan == null)
                return NotFound();

            return View(calisan);
        }

        // POST: Calisan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var calisan = await _context.Calisanlar.FindAsync(id);
            if (calisan != null)
            {
                _context.Calisanlar.Remove(calisan);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CalisanExists(int id)
        {
            return _context.Calisanlar.Any(e => e.Id == id);
        }

        // ------------------ Çalışma Saatleri CRUD Eklemleri ------------------

        // GET: Calisan/CalismaSaatleri/5
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> CalismaSaatleri(int? id)
        {
            if (id == null)
                return NotFound();

            var calisan = await _context.Calisanlar
                .Include(c => c.CalisanCalismaSaatleri)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (calisan == null)
                return NotFound();

            return View(calisan);
        }

        // GET: Calisan/CreateCalismaSaati/5
        [Authorize(Roles = UserRoles.Role_Admin)]
        public IActionResult CreateCalismaSaati(int? calisanId)
        {
            if (calisanId == null)
                return NotFound();

            var calisan = _context.Calisanlar.Find(calisanId);
            if (calisan == null)
                return NotFound();

            var model = new CalisanCalismaSaatleri
            {
                CalisanId = calisan.Id
            };

            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", calisan.Id);
            return View(model);
        }

        // POST: Calisan/CreateCalismaSaati
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> CreateCalismaSaati([Bind("CalisanId,Gun,BaslangicSaati,BitisSaati")] CalisanCalismaSaatleri calisanCalismaSaatleri)
        {
            if (ModelState.IsValid)
            {
                // Çakışma Kontrolü
                bool isOverlap = await _context.CalisanCalismaSaatleri
                    .AnyAsync(ccs => ccs.CalisanId == calisanCalismaSaatleri.CalisanId &&
                                  ccs.Gun == calisanCalismaSaatleri.Gun &&
                                  ((calisanCalismaSaatleri.BaslangicSaati >= ccs.BaslangicSaati && calisanCalismaSaatleri.BaslangicSaati < ccs.BitisSaati) ||
                                   (calisanCalismaSaatleri.BitisSaati > ccs.BaslangicSaati && calisanCalismaSaatleri.BitisSaati <= ccs.BitisSaati) ||
                                   (calisanCalismaSaatleri.BaslangicSaati <= ccs.BaslangicSaati && calisanCalismaSaatleri.BitisSaati >= ccs.BitisSaati)));

                if (isOverlap)
                {
                    ModelState.AddModelError("", "Belirtilen gün ve saatlerde zaten bir çalışma saati mevcut.");
                    ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", calisanCalismaSaatleri.CalisanId);
                    return View(calisanCalismaSaatleri);
                }

                _context.CalisanCalismaSaatleri.Add(calisanCalismaSaatleri);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CalismaSaatleri), new { id = calisanCalismaSaatleri.CalisanId });
            }

            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", calisanCalismaSaatleri.CalisanId);
            return View(calisanCalismaSaatleri);
        }

        // GET: Calisan/EditCalismaSaati/5
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> EditCalismaSaati(int? id)
        {
            if (id == null)
                return NotFound();

            var calisanCalismaSaatleri = await _context.CalisanCalismaSaatleri.FindAsync(id);
            if (calisanCalismaSaatleri == null)
                return NotFound();

            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", calisanCalismaSaatleri.CalisanId);
            return View(calisanCalismaSaatleri);
        }

        // POST: Calisan/EditCalismaSaati/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> EditCalismaSaati(int id, [Bind("Id,CalisanId,Gun,BaslangicSaati,BitisSaati")] CalisanCalismaSaatleri calisanCalismaSaatleri)
        {
            if (id != calisanCalismaSaatleri.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Çakışma Kontrolü
                    bool isOverlap = await _context.CalisanCalismaSaatleri
                        .AnyAsync(ccs => ccs.CalisanId == calisanCalismaSaatleri.CalisanId &&
                                      ccs.Gun == calisanCalismaSaatleri.Gun &&
                                      ccs.Id != calisanCalismaSaatleri.Id && // Kendini kontrol etme
                                      ((calisanCalismaSaatleri.BaslangicSaati >= ccs.BaslangicSaati && calisanCalismaSaatleri.BaslangicSaati < ccs.BitisSaati) ||
                                       (calisanCalismaSaatleri.BitisSaati > ccs.BaslangicSaati && calisanCalismaSaatleri.BitisSaati <= ccs.BitisSaati) ||
                                       (calisanCalismaSaatleri.BaslangicSaati <= ccs.BaslangicSaati && calisanCalismaSaatleri.BitisSaati >= ccs.BitisSaati)));

                    if (isOverlap)
                    {
                        ModelState.AddModelError("", "Belirtilen gün ve saatlerde zaten bir çalışma saati mevcut.");
                        ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", calisanCalismaSaatleri.CalisanId);
                        return View(calisanCalismaSaatleri);
                    }

                    _context.Update(calisanCalismaSaatleri);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CalisanCalismaSaatleriExists(calisanCalismaSaatleri.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(CalismaSaatleri), new { id = calisanCalismaSaatleri.CalisanId });
            }

            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", calisanCalismaSaatleri.CalisanId);
            return View(calisanCalismaSaatleri);
        }

        // GET: Calisan/DeleteCalismaSaati/5
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> DeleteCalismaSaati(int? id)
        {
            if (id == null)
                return NotFound();

            var calisanCalismaSaatleri = await _context.CalisanCalismaSaatleri
                .Include(ccs => ccs.Calisan)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calisanCalismaSaatleri == null)
                return NotFound();

            return View(calisanCalismaSaatleri);
        }

        // POST: Calisan/DeleteCalismaSaati/5
        [HttpPost, ActionName("DeleteCalismaSaati")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Role_Admin)]
        public async Task<IActionResult> DeleteCalismaSaatiConfirmed(int id)
        {
            var calisanCalismaSaatleri = await _context.CalisanCalismaSaatleri.FindAsync(id);
            if (calisanCalismaSaatleri != null)
            {
                int calisanId = calisanCalismaSaatleri.CalisanId;
                _context.CalisanCalismaSaatleri.Remove(calisanCalismaSaatleri);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CalismaSaatleri), new { id = calisanId });
            }
            return NotFound();
        }

        private bool CalisanCalismaSaatleriExists(int id)
        {
            return _context.CalisanCalismaSaatleri.Any(e => e.Id == id);
        }
    }
}
