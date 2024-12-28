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
                // Musteri -> sadece kendi randevuları
                randevular = randevular.Where(r => r.KullaniciId == user.Id);
            }
            else if (roles.Contains("Calisan"))
            {
                // Calisan -> sadece kendine (CalisanId) ait randevular
                var calisan = await _context.Calisanlar
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

                if (calisan != null)
                {
                    randevular = randevular.Where(r => r.CalisanId == calisan.Id);
                }
                else
                {
                    // Bu calisan tablosuna eklenmemiş => hiçbir randevu göremeyecek
                    randevular = randevular.Where(r => false);
                }
            }
            else if (roles.Contains("Admin"))
            {
                // Admin -> hepsini gör
                // randevular = randevular (aynı kalır)
            }

            var list = await randevular.OrderBy(r => r.RandevuTarihi).ToListAsync();
            return View(list);
        }

        // GET: Randevu/Create
        [HttpGet]
        [Authorize(Roles = "Musteri")]
        public IActionResult Create()
        {

            // Müşteri form açmadan önce Calisan/Hizmet listeleri
            ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad");
            ViewData["HizmetId"] = new MultiSelectList(
        _context.Hizmetler.Select(h => new
        {
            h.Id,
            Ad = $"{h.Ad} ({h.SureDakika} dk, {h.Fiyat.ToString("C")})"
        }),
        "Id",
        "Ad"
    );
            return View();
        }

        // POST: Randevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Musteri")]
        public async Task<IActionResult> Create([Bind("CalisanId,RandevuTarihi,HizmetIds")] RandevuViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // ModelState hatalıysa formu tekrar doldur
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            // 1) Seçili hizmetler
            var seciliHizmetler = await _context.Hizmetler
                .Where(h => model.HizmetIds.Contains(h.Id))
                .ToListAsync();

            if (seciliHizmetler.Count == 0)
            {
                ModelState.AddModelError("", "En az bir hizmet seçmeniz gerekmektedir.");
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            // 2) Calisan-Hizmet uyumluluğu
            var calisanHizmetIds = await _context.CalisanHizmetler
                 .Where(ch => ch.CalisanId == model.CalisanId && model.HizmetIds.Contains(ch.HizmetId))
                 .Select(ch => ch.HizmetId)
                 .ToListAsync();
            
             if (calisanHizmetIds.Count != model.HizmetIds.Count)
             {
                 ModelState.AddModelError("", "Seçilen hizmetlerden en az birini bu çalışan veremiyor.");
                 ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                 ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                 return View(model);
             }
            

            // 3) Zaman, çakışma ve çalışma saatleri kontrolü
            var toplamSureDakika = seciliHizmetler.Sum(h => h.SureDakika);
            var randevuBaslangic = model.RandevuTarihi;
            var randevuBitis = randevuBaslangic.AddMinutes(toplamSureDakika);

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

            var baslangicSaatSpan = randevuBaslangic.TimeOfDay;
            var bitisSaatSpan = baslangicSaatSpan.Add(TimeSpan.FromMinutes(toplamSureDakika));

            if (baslangicSaatSpan < calisanCalismaSaatleri.BaslangicSaati ||
                bitisSaatSpan > calisanCalismaSaatleri.BitisSaati)
            {
                ModelState.AddModelError("", "Randevu saati çalışanın çalışma saatleri dışında.");
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            // Çakışma kontrolü
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
                ModelState.AddModelError("", "Seçilen zaman dilimi başka bir randevu ile çakışıyor.");
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            // 4) Randevuyu oluştur
            var user = await _userManager.GetUserAsync(User);
            var randevu = new Randevu
            {
                CalisanId = model.CalisanId,
                RandevuTarihi = randevuBaslangic,
                Fiyat = seciliHizmetler.Sum(h => h.Fiyat),
                Durum = RandevuDurumu.Beklemede,
                KullaniciId = user.Id
            };

            foreach (var hizmet in seciliHizmetler)
            {
                randevu.RandevuHizmetler.Add(new RandevuHizmet { HizmetId = hizmet.Id });
            }

            _context.Add(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Randevu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Calisan)
                .Include(r => r.RandevuHizmetler)
                    .ThenInclude(rh => rh.Hizmet)
                .Include(r => r.Kullanici)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            // Müşteri sadece kendi randevusunu görüntüleyebilir
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
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.RandevuHizmetler)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (randevu == null) return NotFound();

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
            if (id != model.Id) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.RandevuHizmetler)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (randevu == null) return NotFound();

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

            if (!ModelState.IsValid)
            {
                // Model hatası -> formu doldur
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            // Seçili hizmetler
            var seciliHizmetler = await _context.Hizmetler
                .Where(h => model.HizmetIds.Contains(h.Id))
                .ToListAsync();

            if (seciliHizmetler.Count == 0)
            {
                ModelState.AddModelError("", "En az bir hizmet seçmeniz gerekmektedir.");
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            // Çalışan bu hizmetleri verebilir mi?
            var calisanHizmetIds = await _context.CalisanHizmetler
                .Where(ch => ch.CalisanId == model.CalisanId && model.HizmetIds.Contains(ch.HizmetId))
                .Select(ch => ch.HizmetId)
                .ToListAsync();

            if (calisanHizmetIds.Count != model.HizmetIds.Count)
            {
                ModelState.AddModelError("", "Seçilen hizmetlerden en az birini bu çalışan veremiyor.");
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            // Süre ve çalışma saatleri
            var toplamSureDakika = seciliHizmetler.Sum(h => h.SureDakika);
            var randevuBaslangic = model.RandevuTarihi;
            var randevuBitis = randevuBaslangic.AddMinutes(toplamSureDakika);

            var calisanCalismaSaatleri = await _context.CalisanCalismaSaatleri
                .Where(ccs => ccs.CalisanId == model.CalisanId &&
                              ccs.Gun == randevuBaslangic.DayOfWeek)
                .FirstOrDefaultAsync();

            if (calisanCalismaSaatleri == null)
            {
                ModelState.AddModelError("", "Seçilen çalışanın bu gün çalışma saatleri tanımlanmamıştır.");
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            var baslangicSaatSpan = randevuBaslangic.TimeOfDay;
            var bitisSaatSpan = baslangicSaatSpan.Add(TimeSpan.FromMinutes(toplamSureDakika));

            if (baslangicSaatSpan < calisanCalismaSaatleri.BaslangicSaati ||
                bitisSaatSpan > calisanCalismaSaatleri.BitisSaati)
            {
                ModelState.AddModelError("", "Randevu saati çalışanın çalışma saatleri dışında.");
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            // Çakışma
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
                ModelState.AddModelError("", "Seçilen zaman dilimi başka bir randevu ile çakışıyor.");
                ViewData["CalisanId"] = new SelectList(_context.Calisanlar.Where(c => c.AktifMi), "Id", "AdSoyad", model.CalisanId);
                ViewData["HizmetId"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", model.HizmetIds);
                return View(model);
            }

            // Güncelleme
            randevu.CalisanId = model.CalisanId;
            randevu.RandevuTarihi = randevuBaslangic;
            randevu.Fiyat = seciliHizmetler.Sum(h => h.Fiyat);

            _context.RandevuHizmetler.RemoveRange(randevu.RandevuHizmetler);
            foreach (var hizmet in seciliHizmetler)
            {
                randevu.RandevuHizmetler.Add(new RandevuHizmet { HizmetId = hizmet.Id });
            }

            _context.Update(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Randevu/Delete/5 (Müşteri)
        [Authorize(Roles = "Musteri")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Calisan)
                .Include(r => r.RandevuHizmetler)
                    .ThenInclude(rh => rh.Hizmet)
                .Include(r => r.Kullanici)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

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
            if (randevu == null) return NotFound();

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

        // Calisan veya Admin randevuyu onaylayabilir
        [HttpPost]
        [Authorize(Roles = "Calisan,Admin")]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            if (randevu.Durum != RandevuDurumu.Beklemede)
            {
                return BadRequest("Sadece beklemedeki randevular onaylanabilir.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Calisan"))
            {
                var calisan = await _context.Calisanlar
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

                if (calisan == null)
                    return Forbid();

                if (randevu.CalisanId != calisan.Id)
                {
                    return BadRequest("Bu randevu başka bir çalışana ait, onaylayamazsınız.");
                }
            }

            randevu.Durum = RandevuDurumu.Onaylandi;
            _context.Randevular.Update(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Calisan veya Admin randevuyu iptal edebilir
        [HttpPost]
        [Authorize(Roles = "Calisan,Admin")]
        public async Task<IActionResult> IptalEt(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            if (randevu.Durum != RandevuDurumu.Beklemede && randevu.Durum != RandevuDurumu.Onaylandi)
            {
                return BadRequest("Sadece beklemede veya onaylanmış randevular iptal edilebilir.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Calisan"))
            {
                var calisan = await _context.Calisanlar
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

                if (calisan == null)
                    return Forbid();

                if (randevu.CalisanId != calisan.Id)
                {
                    return BadRequest("Bu randevu başka bir çalışana ait, iptal edemezsiniz.");
                }
            }

            randevu.Durum = RandevuDurumu.IptalEdildi;
            _context.Randevular.Update(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
