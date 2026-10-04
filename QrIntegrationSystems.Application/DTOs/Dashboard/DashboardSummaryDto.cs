namespace QrIntegrationSystems.Application.DTOs.Dashboard
{
    // SuperAdmin dashboard ekranındaki özet istatistikler.
    // Businesses ve QRScans tablolarından aggregate sorgularla doldurulur.
    public class DashboardSummaryDto
    {
        // İşletme istatistikleri
        public int TotalBusinesses { get; set; }
        public int ActiveBusinesses { get; set; }
        public int InactiveBusinesses { get; set; }

        // QR tarama istatistikleri
        public int TotalQRScans { get; set; }
        public int TodayQRScans { get; set; }
        public int ThisMonthQRScans { get; set; }

        // Ciro ve Gelir istatistikleri (Abonelikler)
        public decimal TotalRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal TodayRevenue { get; set; }

        // Aylık ciro dağılımı — gelir grafiği için (Key: "yyyy-MM", Value: Tutar)
        public Dictionary<string, decimal> MonthlyRevenueStats { get; set; } = new();

        // Son 7 günlük günlük tarama dağılımı — grafik için
        // Key: Tarih (yyyy-MM-dd), Value: Tarama sayısı
        public Dictionary<string, int> DailyScanStats { get; set; } = new();
    }
}
