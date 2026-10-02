using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [Authorize(Roles = "SuperAdmin")] // Dashboard istatistiklerini sadece SuperAdmin görebilir
    [ApiController]
    [Route("api/[controller]")]       // Adres: /api/dashboard
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // ─────────────────────────────────────────
        // SUPERADMIN GENEL İSTATİSTİK ÖZETİ
        // GET /api/dashboard/summary
        // ─────────────────────────────────────────
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _dashboardService.GetSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
