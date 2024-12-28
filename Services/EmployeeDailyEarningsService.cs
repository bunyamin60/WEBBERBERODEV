using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEBBERBERODEV.DATA;
using WEBBERBERODEV.Models;

namespace WEBBERBERODEV.Services
{
    public class EmployeeDailyEarningsService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeDailyEarningsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeDailyEarningsViewModel>> GetDailyEarningsAsync(DateTime date)
        {
            // Onaylanmış veya beklemedeki randevuları çekiyoruz
            var earnings = await _context.Randevular
                .Where(r => r.RandevuTarihi.Date == date.Date &&
                            (r.Durum == RandevuDurumu.Onaylandi || r.Durum == RandevuDurumu.Beklemede))
                .GroupBy(r => r.Calisan)
                .Select(g => new EmployeeDailyEarningsViewModel
                {
                    EmployeeId = g.Key.Id,
                    EmployeeName = g.Key.AdSoyad,
                    Date = date.Date,
                    TotalEarnings = g.Sum(r => r.Fiyat),
                    TotalAppointments = g.Count()
                })
                .ToListAsync();

            // Tüm çalışanları kapsamak için, kazanç olmayan çalışanları da ekleyin
            var allEmployees = await _context.Calisanlar.ToListAsync();
            var earningsDict = earnings.ToDictionary(e => e.EmployeeId);
            var result = new List<EmployeeDailyEarningsViewModel>();

            foreach (var employee in allEmployees)
            {
                if (earningsDict.ContainsKey(employee.Id))
                {
                    result.Add(earningsDict[employee.Id]);
                }
                else
                {
                    result.Add(new EmployeeDailyEarningsViewModel
                    {
                        EmployeeId = employee.Id,
                        EmployeeName = employee.AdSoyad,
                        Date = date.Date,
                        TotalEarnings = 0,
                        TotalAppointments = 0
                    });
                }
            }

            return result;
        }
    }
}
