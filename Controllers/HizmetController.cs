using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBBERBERODEV.DATA;
using WEBBERBERODEV.Models;

namespace WEBBERBERODEV.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin rolündeki kullanıcılar erişebilecek
    public class HizmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetController(ApplicationDbContext context)
        {
            _context = context; // DbContext enjekte ediliyor
        }

        public IActionResult Index()
        {
            var hizmetler = _context.Hizmetler.ToList();
            return View(hizmetler); // Hizmet listesini View'e gönder
        }

        public IActionResult Details(int id)
        {
            var hizmet = _context.Hizmetler.FirstOrDefault(h => h.Id == id);
            if (hizmet == null)
                return NotFound();

            return View(hizmet);
        }

        public IActionResult Create()
        {
            return View(); // Boş form
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        Console.WriteLine($"Validation Error: {key} - {error.ErrorMessage}");
                    }
                }
                return View(hizmet);
            }

            try
            {
                hizmet.SalonId = 1; // Varsayılan SalonId
                _context.Hizmetler.Add(hizmet);
                _context.SaveChanges();

                // Başarılı kayıt sonrası listeleme sayfasına yönlendir
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
                return View(hizmet);
            }
        }

        public IActionResult Edit(int id)
        {
            var hizmet = _context.Hizmetler.FirstOrDefault(h => h.Id == id);
            if (hizmet == null)
                return NotFound();

            return View(hizmet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Hizmet hizmet)
        {
            if (ModelState.IsValid)
            {
                _context.Hizmetler.Update(hizmet);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(hizmet);
        }

        public IActionResult Delete(int id)
        {
            var hizmet = _context.Hizmetler.FirstOrDefault(h => h.Id == id);
            if (hizmet == null)
                return NotFound();

            return View(hizmet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var hizmet = _context.Hizmetler.FirstOrDefault(h => h.Id == id);
            if (hizmet == null)
                return NotFound();

            _context.Hizmetler.Remove(hizmet);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
