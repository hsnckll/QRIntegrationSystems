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
            var templates = await _templateService.GetAllAsync();
            return Ok(User.IsInRole("SuperAdmin") ? templates : templates.Where(t => t.EntryUrl != null).ToList());
        }

        // ─────────────────────────────────────────
        // YENİ ŞABLON OLUŞTUR (SuperAdmin)
        // POST /api/template
        // ─────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] TemplateCreateDto dto)
        {
            var template = await _templateService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id = template.Id }, template);
        }

        // ─────────────────────────────────────────
        // ŞABLON SİL (SuperAdmin)
        // DELETE /api/template/{id}
        // ─────────────────────────────────────────
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _templateService.DeleteAsync(id);
            return Ok(new { message = "Şablon başarıyla silindi." });
        }
    }
}
