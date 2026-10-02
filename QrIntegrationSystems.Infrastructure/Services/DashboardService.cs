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

            // 3. SON 7 GÜNLÜK GRAFİK VERİSİ
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
                DailyScanStats = dailyStats
            };
        }
    }
}