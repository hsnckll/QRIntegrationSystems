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
        private readonly IBusinessScopeResolver _scope;
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService, IBusinessScopeResolver scope)
        {
            _scope = scope;
            _categoryService = categoryService;
        }

        // ─────────────────────────────────────────
        // 1. İŞLETMENİN TÜM KATEGORİLERİNİ LİSTELE
        // GET /api/category
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int businessId = _scope.ResolveSelectedBusiness();
            var categories = await _categoryService.GetAllAsync(businessId);
            return Ok(categories);
        }

        // ─────────────────────────────────────────
        // 2. TEK KATEGORİ DETAYINI GETİR
        // GET /api/category/{id}
        // ─────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            var category = await _categoryService.GetByIdAsync(businessId, id);
            return Ok(category);
        }

        // ─────────────────────────────────────────
        // 3. YENİ KATEGORİ EKLE
        // POST /api/category
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            var result = await _categoryService.CreateAsync(businessId, dto);
            return Ok(result);
        }

        // ─────────────────────────────────────────
        // 4. KATEGORİ GÜNCELLE
        // PUT /api/category/{id}
        // ─────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCategoryDto dto)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            var result = await _categoryService.UpdateAsync(businessId, id, dto);
            return Ok(result);
        }

        // ─────────────────────────────────────────
        // 5. KATEGORİ SİL (Soft Delete)
        // DELETE /api/category/{id}
        // ─────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            await _categoryService.DeleteAsync(businessId, id);
            return Ok(new { message = "Kategori ve bağlı ürünler başarıyla silindi." });
        }

        // ─────────────────────────────────────────
        // 6. KATEGORİYİ AKTİF/PASİF YAP (Switch Butonu)
        // PATCH /api/category/{id}/toggle-active
        // ─────────────────────────────────────────
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive([FromRoute] int id)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            await _categoryService.ToggleActiveAsync(businessId, id);
            return Ok(new { message = "Kategori aktiflik durumu güncellendi." });
        }

    }
}