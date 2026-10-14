using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.DTOs.Product;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Business")] // SuperAdmin ve Business; işletme kapsamı ortak resolver ile doğrulanır.
    [ApiController]
    [Route("api/[controller]")]     // Adres: /api/product
    public class ProductController : ControllerBase
    {
        private readonly IBusinessScopeResolver _scope;
        private readonly IProductService _productService;
        private readonly IFileService _files;

        public ProductController(IProductService productService, IFileService files, IBusinessScopeResolver scope)
        {
            _scope = scope;
            _productService = productService;
            _files = files;
        }

        // ─────────────────────────────────────────
        // 1. İŞLETMENİN TÜM ÜRÜNLERİNİ LİSTELE
        // GET /api/product
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int businessId = _scope.ResolveSelectedBusiness();
            var products = await _productService.GetAllAsync(businessId);
            return Ok(products);
        }

        // ─────────────────────────────────────────
        // 2. TEK ÜRÜN DETAYI
        // GET /api/product/{id}
        // ─────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            var product = await _productService.GetByIdAsync(businessId, id);
            return Ok(product);
        }

        // ─────────────────────────────────────────
        // 3. YENİ ÜRÜN OLUŞTUR
        // POST /api/product
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            var result = await _productService.CreateAsync(businessId, dto);
            return Ok(result);
        }

        // ─────────────────────────────────────────
        // 4. ÜRÜN GÜNCELLE (İsim, Fiyat, Kategori vs.)
        // PUT /api/product/{id}
        // ─────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateProductDto dto)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            var result = await _productService.UpdateAsync(businessId, id, dto);
            return Ok(result);
        }

        // ─────────────────────────────────────────
        // 5. ÜRÜN SİL (Soft Delete)
        // DELETE /api/product/{id}
        // ─────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            await _productService.DeleteAsync(businessId, id);
            return Ok(new { message = "Ürün başarıyla silindi." });
        }

        // ─────────────────────────────────────────
        // 6. MENÜDE GÖSTER/GİZLE (Switch Butonu)
        // PATCH /api/product/{id}/toggle-active
        // ─────────────────────────────────────────
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive([FromRoute] int id)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            await _productService.ToggleActiveAsync(businessId, id);
            return Ok(new { message = "Ürün aktiflik durumu güncellendi." });
        }

        // ─────────────────────────────────────────
        // 7. ÜRÜN RESMİ YOLUNU GÜNCELLE
        // PATCH /api/product/{id}/image
        // ─────────────────────────────────────────
        [HttpPatch("{id}/image")]
        public async Task<IActionResult> UpdateImage([FromRoute] int id, [FromBody] UpdateProductImageRequest request)
        {
            int businessId = _scope.ResolveSelectedBusiness();
            await _files.EnsureImageAccessAsync(request.ImagePath, businessId, User.IsInRole("SuperAdmin"));
            await _productService.UpdateImageAsync(businessId, id, request.ImagePath);
            return Ok(new { message = "Ürün görseli başarıyla güncellendi." });
        }

    }

    // Ürün görsel yolu güncellemek için kullanılan küçük istek modeli
    public class UpdateProductImageRequest
    {
        public string ImagePath { get; set; } = null!;
    }
}