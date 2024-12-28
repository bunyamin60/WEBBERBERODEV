using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WEBBERBERODEV.DATA;
using WEBBERBERODEV.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace WEBBERBERODEV.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomePageViewModel
            {
                SalonAdi = "B�N BERBER",
                AcilisKapanisSaati = "10:00 - 20:00",
                Hizmetler = await _context.Hizmetler.ToListAsync(),
                Calisanlar = await _context.Calisanlar
                                    .Where(c => c.AktifMi)
                                    .Include(c => c.ApplicationUser)
                                    .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
