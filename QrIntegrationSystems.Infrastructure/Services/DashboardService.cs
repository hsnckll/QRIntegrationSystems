using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.DTOs.Dashboard;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;

        public DashboardService(AppDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────
        // SUPERADMIN DASHBOARD İSTATİSTİKLERİ
        // ─────────────────────────────────────────
        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var now = DateTime.UtcNow;

            // Bugünün başlangıcı (Gece 00:00:00)
            var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

            // Bu ayın başlangıcı (Ayın 1'i Gece 00:00:00)
            var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Son 7 günün başlangıcı (Bugün dahil geriye 6 gün)
            var sevenDaysAgo = todayStart.AddDays(-6);

            // 1. İŞLETME İSTATİSTİKLERİ
            var totalBusinesses = await _db.Businesses
                .CountAsync(b => b.IsDeleted == false);

            var activeBusinesses = await _db.Businesses
                .CountAsync(b => b.IsDeleted == false && b.IsActive == true);

            var inactiveBusinesses = await _db.Businesses
                .CountAsync(b => b.IsDeleted == false && b.IsActive == false);

            // 2. QR TARAMA GENEL İSTATİSTİKLERİ
            var totalScans = await _db.QRScans.CountAsync();

            var todayScans = await _db.QRScans
                .CountAsync(s => s.ScannedAt >= todayStart);

            var thisMonthScans = await _db.QRScans
                .CountAsync(s => s.ScannedAt >= thisMonthStart);

            // 3. ABONELİK CİRO VE GELİR İSTATİSTİKLERİ
            var totalRevenue = await _db.Subscriptions
                .Where(s => s.IsDeleted == false)
                .SumAsync(s => s.AmountPaid ?? 0);

            var todayRevenue = await _db.Subscriptions
                .Where(s => s.IsDeleted == false && s.StartDate >= todayStart)
                .SumAsync(s => s.AmountPaid ?? 0);

            var thisMonthRevenue = await _db.Subscriptions
                .Where(s => s.IsDeleted == false && s.StartDate >= thisMonthStart)
                .SumAsync(s => s.AmountPaid ?? 0);

            // 4. AYLIK CİRO GRAFİK VERİSİ (Son 12 ay)
            var twelveMonthsAgo = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);

            var recentSubscriptions = await _db.Subscriptions
                .Where(s => s.IsDeleted == false && s.StartDate >= twelveMonthsAgo)
                .Select(s => new { s.StartDate, Amount = s.AmountPaid ?? 0 })
                .ToListAsync();

            var monthlyRevenueStats = new Dictionary<string, decimal>();

            for (int i = 0; i < 12; i++)
            {
                var currentMonth = twelveMonthsAgo.AddMonths(i);
                string monthKey = currentMonth.ToString("yyyy-MM");

                decimal monthSum = recentSubscriptions
                    .Where(s => s.StartDate.Year == currentMonth.Year && s.StartDate.Month == currentMonth.Month)
                    .Sum(s => s.Amount);

                monthlyRevenueStats.Add(monthKey, monthSum);
            }

            // 5. SON 7 GÜNLÜK QR TARAMA GRAFİK VERİSİ
            // Son 7 günde yapılan taramaların tarihlerini çekiyoruz
            var recentScans = await _db.QRScans
                .Where(s => s.ScannedAt >= sevenDaysAgo)
                .Select(s => s.ScannedAt.Date)
                .ToListAsync();

            // Günleri 0 ile başlatıyoruz ki hiç tarama olmayan günler grafikte boşluk oluşturmasın
            var dailyStats = new Dictionary<string, int>();

            for (int i = 0; i < 7; i++)
            {
                var currentDay = sevenDaysAgo.AddDays(i);
                string dateKey = currentDay.ToString("yyyy-MM-dd"); // Örn: "2026-10-02"

                // O güne denk gelen taramaları say
                int countForDay = recentScans.Count(date => date == currentDay.Date);
                dailyStats.Add(dateKey, countForDay);
            }

            // DTO olarak paketleyip dönüyoruz
            return new DashboardSummaryDto
            {
                TotalBusinesses = totalBusinesses,
                ActiveBusinesses = activeBusinesses,
                InactiveBusinesses = inactiveBusinesses,
                TotalQRScans = totalScans,
                TodayQRScans = todayScans,
                ThisMonthQRScans = thisMonthScans,
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                ThisMonthRevenue = thisMonthRevenue,
                MonthlyRevenueStats = monthlyRevenueStats,
                DailyScanStats = dailyStats
            };
        }
    }
}