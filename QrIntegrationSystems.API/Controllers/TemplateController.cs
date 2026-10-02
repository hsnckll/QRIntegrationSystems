using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [Authorize] // Giriş yapmış hem SuperAdmin hem Business temaları listeleyebilir
    [ApiController]
    [Route("api/[controller]")] // Adres: /api/template
    public class TemplateController : ControllerBase
    {
        private readonly ITemplateService _templateService;

        public TemplateController(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        // ─────────────────────────────────────────
        // TÜM ŞABLONLARI LİSTELE (Dropdown İçin)
        // GET /api/template
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var templates = await _templateService.GetAllAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
