using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.DTOs.Category;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Business")] // Sadece Business rolüne sahip giriş yapmış kişiler bu kapıyı çalabilir!
    [ApiController]
    [Route("api/[controller]")]     // Adres: /api/category
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // ─────────────────────────────────────────
        // 1. İŞLETMENİN TÜM KATEGORİLERİNİ LİSTELE
        // GET /api/category
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var categories = await _categoryService.GetAllAsync(businessId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 2. TEK KATEGORİ DETAYINI GETİR
        // GET /api/category/{id}
        // ─────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var category = await _categoryService.GetByIdAsync(businessId, id);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 3. YENİ KATEGORİ EKLE
        // POST /api/category
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var result = await _categoryService.CreateAsync(businessId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 4. KATEGORİ GÜNCELLE
        // PUT /api/category/{id}
        // ─────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var result = await _categoryService.UpdateAsync(businessId, id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 5. KATEGORİ SİL (Soft Delete)
        // DELETE /api/category/{id}
        // ─────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                await _categoryService.DeleteAsync(businessId, id);
                return Ok(new { message = "Kategori ve bağlı ürünler başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 6. KATEGORİYİ AKTİF/PASİF YAP (Switch Butonu)
        // PATCH /api/category/{id}/toggle-active
        // ─────────────────────────────────────────
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive([FromRoute] int id)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                await _categoryService.ToggleActiveAsync(businessId, id);
                return Ok(new { message = "Kategori aktiflik durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // YARDIMCI METOT: JWT Token'dan BusinessId'yi Güvenli Şekilde Okur
        // ─────────────────────────────────────────
        private int GetCurrentBusinessId()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                if (!int.TryParse(Request.Query["businessId"], out var selectedId) || selectedId <= 0)
                    throw new UnauthorizedAccessException("İşletme seçimi gerekli.");
                return selectedId;
            }
            // Business users always use their own claim, even if a query parameter is supplied.
            var claim = User.Claims.FirstOrDefault(c => c.Type == "BusinessId");

            if (claim == null || int.TryParse(claim.Value, out int businessId) == false)
            {
                throw new UnauthorizedAccessException("Oturum bilgileriniz doğrulanamadı.");
            }

            return businessId;
        }
    }
}