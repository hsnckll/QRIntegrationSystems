using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.DTOs.Business;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Adres: /api/business
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessScopeResolver _scope;
        private readonly IBusinessService _businessService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IFileService _files;

        public BusinessController(IBusinessService businessService, IQrCodeService qrCodeService, IFileService files, IBusinessScopeResolver scope)
        {
            _scope = scope;
            _businessService = businessService;
            _qrCodeService = qrCodeService;
            _files = files;
        }

        // ─────────────────────────────────────────
        // 1. TÜM İŞLETMELERİ LİSTELE (Sadece SuperAdmin)
        // GET /api/business
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var businesses = await _businessService.GetAllAsync();
            return Ok(businesses);
        }

        // ─────────────────────────────────────────
        // 2. TEK İŞLETME DETAYI (SuperAdmin İçin)
        // GET /api/business/{id}
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var business = await _businessService.GetByIdAsync(id);
            return Ok(business);
        }

        // ─────────────────────────────────────────
        // 3. İŞLETMENİN KENDİ PROFİLİNİ GETİRMESİ (İşletme Paneli İçin)
        // GET /api/business/profile
        // ─────────────────────────────────────────
        [Authorize(Roles = "Business")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int businessId = _scope.OwnBusinessId();
            var business = await _businessService.GetByIdAsync(businessId);
            return Ok(business);
        }

        // ─────────────────────────────────────────
        // 4. YENİ İŞLETME OLUŞTUR (Sadece SuperAdmin)
        // POST /api/business
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBusinessDto dto)
        {
            // 1. İşletmeyi kaydet
            var newBusiness = await _businessService.CreateAsync(dto);

            // 2. İşletme oluşur oluşmaz otomatik olarak ilk QR kodunu üret ve kaydet!
            await _qrCodeService.GenerateAsync(newBusiness.Id, newBusiness.Slug);

            // Güncel QR koduyla birlikte işletmenin detayını dön
            var finalDetail = await _businessService.GetByIdAsync(newBusiness.Id);
            return Ok(finalDetail);
        }

        // ─────────────────────────────────────────
        // 5. İŞLETME BİLGİLERİNİ GÜNCELLE
        // PUT /api/business/{id}
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Business")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateBusinessDto dto)
        {
            // Güvenlik: Eğer giriş yapan bir işletmeyse, başkasının işletmesini güncelleyemez!
            _scope.EnsureAccess(id);

            var result = await _businessService.UpdateAsync(id, dto);
            return Ok(result);
        }

        // ─────────────────────────────────────────
        // 6. İŞLETMEYİ SİL (Soft Delete - Sadece SuperAdmin)
        // DELETE /api/business/{id}
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _businessService.DeleteAsync(id);
            return Ok(new { message = "İşletme başarıyla silindi." });
        }

        // ─────────────────────────────────────────
        // 7. İŞLETMEYİ DONDUR / AKTİFLEŞTİR (Sadece SuperAdmin)
        // PATCH /api/business/{id}/toggle-active
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive([FromRoute] int id)
        {
            await _businessService.ToggleActiveAsync(id);
            return Ok(new { message = "İşletmenin aktiflik durumu güncellendi." });
        }

        // ─────────────────────────────────────────
        // 8. LOGO YOLUNU GÜNCELLE
        // PATCH /api/business/{id}/logo
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Business")]
        [HttpPatch("{id}/logo")]
        public async Task<IActionResult> UpdateLogo([FromRoute] int id, [FromBody] UpdateImageRequest request)
        {
            _scope.EnsureAccess(id);
            await _files.EnsureImageAccessAsync(request.ImagePath, id, User.IsInRole("SuperAdmin"));
            await _businessService.UpdateLogoAsync(id, request.ImagePath);
            return Ok(new { message = "Logo başarıyla güncellendi." });
        }

        // ─────────────────────────────────────────
        // 9. BANNER YOLUNU GÜNCELLE
        // PATCH /api/business/{id}/banner
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Business")]
        [HttpPatch("{id}/banner")]
        public async Task<IActionResult> UpdateBanner([FromRoute] int id, [FromBody] UpdateImageRequest request)
        {
            _scope.EnsureAccess(id);
            await _files.EnsureImageAccessAsync(request.ImagePath, id, User.IsInRole("SuperAdmin"));
            await _businessService.UpdateBannerAsync(id, request.ImagePath);
            return Ok(new { message = "Banner başarıyla güncellendi." });
        }

        // ─────────────────────────────────────────
        // 10. İŞLETMENİN KENDİ QR TARAMA İSTATİSTİKLERİ
        // GET /api/business/statistics
        // ─────────────────────────────────────────
        [Authorize(Roles = "Business")]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            int businessId = _scope.OwnBusinessId();
            var stats = await _businessService.GetStatisticsAsync(businessId);
            return Ok(stats);
        }

        // ─────────────────────────────────────────
        // 11. QR KODU YENİDEN ÜRET (SuperAdmin)
        // POST /api/business/{id}/regenerate-qr
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("{id}/regenerate-qr")]
        public async Task<IActionResult> RegenerateQr([FromRoute] int id)
        {
            var business = await _businessService.GetByIdAsync(id);
            if (business == null)
            {
                return NotFound(new { message = "İşletme bulunamadı." });
            }

            var qrPath = await _qrCodeService.GenerateAsync(id, business.Slug);
            return Ok(new { message = "QR kod başarıyla yeniden üretildi.", qrImagePath = qrPath });
        }

    }

    public class UpdateImageRequest
    {
        public string ImagePath { get; set; } = null!;
    }
}