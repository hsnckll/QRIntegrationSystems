using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.DTOs.Product;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Business")] // Sadece Business rolündeki işletmeler erişebilir
    [ApiController]
    [Route("api/[controller]")]     // Adres: /api/product
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // ─────────────────────────────────────────
        // 1. İŞLETMENİN TÜM ÜRÜNLERİNİ LİSTELE
        // GET /api/product
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var products = await _productService.GetAllAsync(businessId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 2. TEK ÜRÜN DETAYI
        // GET /api/product/{id}
        // ─────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var product = await _productService.GetByIdAsync(businessId, id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 3. YENİ ÜRÜN OLUŞTUR
        // POST /api/product
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var result = await _productService.CreateAsync(businessId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 4. ÜRÜN GÜNCELLE (İsim, Fiyat, Kategori vs.)
        // PUT /api/product/{id}
        // ─────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateProductDto dto)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var result = await _productService.UpdateAsync(businessId, id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 5. ÜRÜN SİL (Soft Delete)
        // DELETE /api/product/{id}
        // ─────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                await _productService.DeleteAsync(businessId, id);
                return Ok(new { message = "Ürün başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 6. MENÜDE GÖSTER/GİZLE (Switch Butonu)
        // PATCH /api/product/{id}/toggle-active
        // ─────────────────────────────────────────
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive([FromRoute] int id)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                await _productService.ToggleActiveAsync(businessId, id);
                return Ok(new { message = "Ürün aktiflik durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 7. ÜRÜN RESMİ YOLUNU GÜNCELLE
        // PATCH /api/product/{id}/image
        // ─────────────────────────────────────────
        [HttpPatch("{id}/image")]
        public async Task<IActionResult> UpdateImage([FromRoute] int id, [FromBody] UpdateProductImageRequest request)
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                await _productService.UpdateImageAsync(businessId, id, request.ImagePath);
                return Ok(new { message = "Ürün görseli başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // YARDIMCI METOT: JWT Token'dan BusinessId Okuma
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

    // Ürün görsel yolu güncellemek için kullanılan küçük istek modeli
    public class UpdateProductImageRequest
    {
        public string ImagePath { get; set; } = null!;
    }
}