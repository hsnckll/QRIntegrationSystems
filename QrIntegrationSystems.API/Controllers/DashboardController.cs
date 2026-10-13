using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.API.Controllers
{
    [Authorize(Roles = "SuperAdmin")] // Dashboard istatistiklerini sadece SuperAdmin görebilir
    [ApiController]
    [Route("api/[controller]")]       // Adres: /api/dashboard
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IBusinessService _businesses;
        private readonly ITemplateService _templates;
        private readonly AppDbContext _db;

        public DashboardController(IDashboardService dashboardService, IBusinessService businesses, ITemplateService templates, AppDbContext db)
        {
            _dashboardService = dashboardService;
            _businesses = businesses; _templates = templates; _db = db;
        }

        [HttpGet("bootstrap")]
        public async Task<IActionResult> Bootstrap()
        {
            try
            {
                var businesses = await _businesses.GetAllDetailsAsync();
                var templates = await _templates.GetAllAsync();
                var summary = await _dashboardService.GetSummaryAsync();
                var subscriptions = await _db.Subscriptions.AsNoTracking().Where(s => !s.IsDeleted)
                    .Select(s => new { s.Id, s.BusinessId, s.StartDate, s.EndDate, s.AmountPaid, s.PaymentMethod, s.Note, s.CreatedAt }).ToListAsync();
                var logs = await _db.Logs.AsNoTracking().OrderByDescending(l => l.CreatedAt).Take(500)
                    .Select(l => new { l.Id, l.BusinessId, l.Action, l.EntityType, l.EntityId, l.ActorType, l.CreatedAt, l.OldValues, l.NewValues }).ToListAsync();
                return Ok(new { businesses, templates, summary, subscriptions, logs });
            }
            catch (Exception ex) { return ApiErrors.FromException(this, ex); }
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
                return ApiErrors.FromException(this, ex);
            }
        }
    }
}
