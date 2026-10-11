using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.DTOs.Template;
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

        // ─────────────────────────────────────────
        // YENİ ŞABLON OLUŞTUR (SuperAdmin)
        // POST /api/template
        // ─────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] TemplateCreateDto dto)
        {
            try
            {
                var template = await _templateService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { id = template.Id }, template);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // ŞABLON SİL (SuperAdmin)
        // DELETE /api/template/{id}
        // ─────────────────────────────────────────
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _templateService.DeleteAsync(id);
                return Ok(new { message = "Şablon başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
