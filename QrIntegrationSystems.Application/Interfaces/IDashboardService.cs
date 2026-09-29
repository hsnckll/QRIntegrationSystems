using QrIntegrationSystems.Application.DTOs.Dashboard;

namespace QrIntegrationSystems.Application.Interfaces
{
    // SuperAdmin dashboard'u için aggregate istatistik sorguları.
    public interface IDashboardService
    {
        // Businesses ve QRScans tablolarından toplam verileri çeker
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}
