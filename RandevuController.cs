using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBERBERODEV.DATA;
using WEBBERBERODEV.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WEBBERBERODEV.Controllers
{
    [Authorize(Roles = "Musteri,Calisan,Admin")]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RandevuController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Randevu
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            IQueryable<Randevu> randevular = _context.Randevular
                .Include(r => r.Calisan)
                .Include(r => r.RandevuHizmetler)
                    .ThenInclude(rh => rh.Hizmet)
                .Include(r => r.Kullanici);

            if (roles.Contains("Musteri"))
            {
                // Musteri ise sadece kendi randevularını gör
                randevular = randevular.Where(r => r.KullaniciId == user.Id);
            }

            // Admin ve Calisan ise tüm randevuları gör
            return View(await randevular.OrderBy(r => r.RandevuTarihi).ToListAsync());
        }

        // GET: Randevu/Create
        [Authorize(Roles = "Musteri")]
        public IActionResult Create()
        {
            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad");
            ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad");
            return View();
        }

        // POST: Randevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Musteri")]
        public async Task<IActionResult> Create([Bind("CalisanId,RandevuTarihi,HizmetIds")] RandevuViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Hizmetleri al
                var hizmetler = await _context.Hizmetler
                    .Where(h => model.HizmetIds.Contains(h.Id))
                    .ToListAsync();

                if (hizmetler.Count == 0)
                {
                    ModelState.AddModelError("", "En az bir hizmet seçmeniz gerekmektedir.");
                    ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                    ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                    return View(model);
                }

                // Toplam süreyi hesapla
                var toplamSureDakika = hizmetler.Sum(h => h.SureDakika);

                // Randevu başlangıç ve bitiş zamanını belirle
                var randevuBaslangic = model.RandevuTarihi;
                var randevuBitis = randevuBaslangic.AddMinutes(toplamSureDakika);

                // Çalışanın çalışma saatlerini al
                var calisanCalismaSaatleri = await _context.CalisanCalismaSaatleri
                    .Where(ccs => ccs.CalisanId == model.CalisanId &&
                                  ccs.Gun == randevuBaslangic.DayOfWeek)
                    .FirstOrDefaultAsync();

                if (calisanCalismaSaatleri == null)
                {
                    ModelState.AddModelError("", "Seçilen çalışanın bu gün için çalışma saatleri tanımlanmamıştır.");
                    ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                    ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                    return View(model);
                }

                var baslangicSaatSpan = model.RandevuTarihi.TimeOfDay;
                var bitisSaatSpan = baslangicSaatSpan.Add(TimeSpan.FromMinutes(toplamSureDakika));

                // Çalışma saatleri içinde olup olmadığını kontrol et
                if (baslangicSaatSpan < calisanCalismaSaatleri.BaslangicSaati ||
                    bitisSaatSpan > calisanCalismaSaatleri.BitisSaati)
                {
                    ModelState.AddModelError("", "Randevu saati çalışanın çalışma saatleri dışında.");
                    ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                    ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                    return View(model);
                }

                // Zaman çakışmasını kontrol et
                var mevcutRandevular = await _context.Randevular
                    .Where(r => r.CalisanId == model.CalisanId &&
                                r.RandevuTarihi.Date == model.RandevuTarihi.Date &&
                                (r.Durum == RandevuDurumu.Onaylandi || r.Durum == RandevuDurumu.Beklemede))
                    .Include(r => r.RandevuHizmetler)
                        .ThenInclude(rh => rh.Hizmet)
                    .ToListAsync();

                bool zamanUyumsuz = mevcutRandevular.Any(r =>
                {
                    var rStart = r.RandevuTarihi.TimeOfDay;
                    var rEnd = rStart.Add(TimeSpan.FromMinutes(r.RandevuHizmetler.Sum(rh => rh.Hizmet.SureDakika)));

                    return (baslangicSaatSpan < rEnd) && (bitisSaatSpan > rStart);
                });

                if (zamanUyumsuz)
                {
                    ModelState.AddModelError("", "Seçilen zaman dilimi mevcut bir randevu ile çakışıyor.");
                    ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                    ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                    return View(model);
                }

                // Randevuyu oluştur
                var user = await _userManager.GetUserAsync(User);

                var randevu = new Randevu
                {
                    CalisanId = model.CalisanId,
                    RandevuTarihi = randevuBaslangic,
                    Fiyat = hizmetler.Sum(h => h.Fiyat),
                    Durum = RandevuDurumu.Beklemede,
                    KullaniciId = user.Id
                };

                foreach (var hizmet in hizmetler)
                {
                    randevu.RandevuHizmetler.Add(new RandevuHizmet
                    {
                        HizmetId = hizmet.Id
                    });
                }

                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Eğer ModelState geçerli değilse, formu tekrar göstermek için return View(model); ifadesi olmalıdır.
            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
            ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
            return View(model);
        }

            // GET: Randevu/Details/5
            public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Calisan)
                .Include(r => r.RandevuHizmetler)
                    .ThenInclude(rh => rh.Hizmet)
                .Include(r => r.Kullanici)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (randevu == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Musteri") && randevu.KullaniciId != user.Id)
            {
                return Forbid();
            }

            return View(randevu);
        }

        // GET: Randevu/Edit/5
        [Authorize(Roles = "Musteri")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.RandevuHizmetler)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (randevu == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (randevu.KullaniciId != user.Id)
            {
                return Forbid();
            }

            if (randevu.Durum != RandevuDurumu.Beklemede)
            {
                return BadRequest("Onaylanmış veya iptal edilmiş randevular düzenlenemez.");
            }

            var selectedHizmetIds = randevu.RandevuHizmetler.Select(rh => rh.HizmetId).ToList();

            var model = new RandevuViewModel
            {
                Id = randevu.Id,
                CalisanId = randevu.CalisanId,
                RandevuTarihi = randevu.RandevuTarihi,
                HizmetIds = selectedHizmetIds
            };

            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", randevu.CalisanId);
            ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", selectedHizmetIds);
            return View(model);
        }

        // POST: Randevu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Musteri")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CalisanId,RandevuTarihi,HizmetIds")] RandevuViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.RandevuHizmetler)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (randevu == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (randevu.KullaniciId != user.Id)
            {
                return Forbid();
            }

            if (randevu.Durum != RandevuDurumu.Beklemede)
            {
                ModelState.AddModelError("", "Onaylanmış veya iptal edilmiş randevular düzenlenemez.");
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Hizmetleri al
                var hizmetler = await _context.Hizmetler
                    .Where(h => model.HizmetIds.Contains(h.Id))
                    .ToListAsync();

                if (hizmetler.Count == 0)
                {
                    ModelState.AddModelError("", "En az bir hizmet seçmeniz gerekmektedir.");
                    ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                    ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                    return View(model);
                }

                // Toplam süreyi hesapla
                var toplamSureDakika = hizmetler.Sum(h => h.SureDakika);

                // Randevu başlangıç ve bitiş zamanını belirle
                var randevuBaslangic = model.RandevuTarihi;
                var randevuBitis = randevuBaslangic.AddMinutes(toplamSureDakika);

                // Çalışanın çalışma saatlerini al
                var calisanCalismaSaatleri = await _context.CalisanCalismaSaatleri
                    .Where(ccs => ccs.CalisanId == model.CalisanId &&
                                  ccs.Gun == randevuBaslangic.DayOfWeek)
                    .FirstOrDefaultAsync();

                if (calisanCalismaSaatleri == null)
                {
                    ModelState.AddModelError("", "Seçilen çalışanın bu gün için çalışma saatleri tanımlanmamıştır.");
                    ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                    ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                    return View(model);
                }

                var baslangicSaatSpan = model.RandevuTarihi.TimeOfDay;
                var bitisSaatSpan = baslangicSaatSpan.Add(TimeSpan.FromMinutes(toplamSureDakika));

                // Çalışma saatleri içinde olup olmadığını kontrol et
                if (baslangicSaatSpan < calisanCalismaSaatleri.BaslangicSaati ||
                    bitisSaatSpan > calisanCalismaSaatleri.BitisSaati)
                {
                    ModelState.AddModelError("", "Randevu saati çalışanın çalışma saatleri dışında.");
                    ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                    ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                    return View(model);
                }

                // Zaman çakışmasını kontrol et
                var mevcutRandevular = await _context.Randevular
                    .Where(r => r.CalisanId == model.CalisanId &&
                                r.RandevuTarihi.Date == model.RandevuTarihi.Date &&
                                (r.Durum == RandevuDurumu.Onaylandi || r.Durum == RandevuDurumu.Beklemede) &&
                                r.Id != model.Id)
                    .Include(r => r.RandevuHizmetler)
                        .ThenInclude(rh => rh.Hizmet)
                    .ToListAsync();

                bool zamanUyumsuz = mevcutRandevular.Any(r =>
                {
                    var rStart = r.RandevuTarihi.TimeOfDay;
                    var rEnd = rStart.Add(TimeSpan.FromMinutes(r.RandevuHizmetler.Sum(rh => rh.Hizmet.SureDakika)));

                    return (baslangicSaatSpan < rEnd) && (bitisSaatSpan > rStart);
                });

                if (zamanUyumsuz)
                {
                    ModelState.AddModelError("", "Seçilen zaman dilimi mevcut bir randevu ile çakışıyor.");
                    ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                    ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                    return View(model);
                }

                // Randevu bilgilerini güncelle
                randevu.CalisanId = model.CalisanId;
                randevu.RandevuTarihi = randevuBaslangic;
                randevu.Fiyat = hizmetler.Sum(h => h.Fiyat);

                // RandevuHizmetler'i güncelle
                _context.RandevuHizmetler.RemoveRange(randevu.RandevuHizmetler);
                foreach (var hizmet in hizmetler)
                {
                    randevu.RandevuHizmetler.Add(new RandevuHizmet
                    {
                        HizmetId = hizmet.Id
                    });
                }

                _context.Update(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
            ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
            return View(model);
        }

        // GET: Randevu/Delete/5
        [Authorize(Roles = "Musteri")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Calisan)
                .Include(r => r.RandevuHizmetler)
                    .ThenInclude(rh => rh.Hizmet)
                .Include(r => r.Kullanici)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (randevu == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (randevu.KullaniciId != user.Id)
            {
                return Forbid();
            }

            if (randevu.Durum != RandevuDurumu.Beklemede)
            {
                return BadRequest("Onaylanmış veya iptal edilmiş randevular silinemez.");
            }

            return View(randevu);
        }

        // POST: Randevu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Musteri")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (randevu.KullaniciId != user.Id)
            {
                return Forbid();
            }

            if (randevu.Durum != RandevuDurumu.Beklemede)
            {
                ModelState.AddModelError("", "Onaylanmış veya iptal edilmiş randevular silinemez.");
                return View(randevu);
            }

            _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

