namespace QrIntegrationSystems.Application.DTOs.Business
{
    // İşletmenin kendi QR okutma istatistikleri ve zaman damgaları
    public class BusinessStatisticsDto
    {
        public int TotalScans { get; set; }
        public int TodayScans { get; set; }
        public int ThisMonthScans { get; set; }
        public List<DateTime> Scans { get; set; } = new();
    }
}
