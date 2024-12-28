using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBBERBERODEV.Services;
using System;
using System.Threading.Tasks;

namespace WEBBERBERODEV.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PerformanceController : Controller
    {
        private readonly EmployeeDailyEarningsService _earningsService;

        public PerformanceController(EmployeeDailyEarningsService earningsService)
        {
            _earningsService = earningsService;
        }

        // GET: Performance/DailyEarnings
        public async Task<IActionResult> DailyEarnings(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var earnings = await _earningsService.GetDailyEarningsAsync(selectedDate);
            ViewBag.SelectedDate = selectedDate;
            return View(earnings);
        }
    }
}
