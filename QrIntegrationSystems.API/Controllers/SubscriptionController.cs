using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.DTOs.Subscription;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [Authorize(Roles = "SuperAdmin")] // Abonelik yönetimini yalnızca SuperAdmin yapabilir
    [ApiController]
    [Route("api/[controller]")]       // Adres: /api/subscription
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        // ─────────────────────────────────────────
        // 1. İŞLETMENİN TÜM ABONELİK GEÇMİŞİ
        // GET /api/subscription/business/{businessId}
        // ─────────────────────────────────────────
        [HttpGet("business/{businessId}")]
        public async Task<IActionResult> GetByBusinessId([FromRoute] int businessId)
        {
            try
            {
                var history = await _subscriptionService.GetByBusinessIdAsync(businessId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 2. İŞLETMENİN GÜNCEL AKTİF ABONELİĞİ
        // GET /api/subscription/business/{businessId}/active
        // ─────────────────────────────────────────
        [HttpGet("business/{businessId}/active")]
        public async Task<IActionResult> GetActive([FromRoute] int businessId)
        {
            try
            {
                var activeSubscription = await _subscriptionService.GetActiveAsync(businessId);
                return Ok(activeSubscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 3. YENİ ABONELİK EKLE
        // POST /api/subscription/business/{businessId}
        // ─────────────────────────────────────────
        [HttpPost("business/{businessId}")]
        public async Task<IActionResult> Create([FromRoute] int businessId, [FromBody] CreateSubscriptionDto dto)
        {
            try
            {
                var result = await _subscriptionService.CreateAsync(businessId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 4. ABONELİK SİL (Soft Delete)
        // DELETE /api/subscription/business/{businessId}/{id}
        // ─────────────────────────────────────────
        [HttpDelete("business/{businessId}/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int businessId, [FromRoute] int id)
        {
            try
            {
                await _subscriptionService.DeleteAsync(businessId, id);
                return Ok(new { message = "Abonelik kaydı başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
