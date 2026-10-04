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
        private readonly IBusinessService _businessService;
        private readonly IQrCodeService _qrCodeService;

        public BusinessController(IBusinessService businessService, IQrCodeService qrCodeService)
        {
            _businessService = businessService;
            _qrCodeService = qrCodeService;
        }

        // ─────────────────────────────────────────
        // 1. TÜM İŞLETMELERİ LİSTELE (Sadece SuperAdmin)
        // GET /api/business
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var businesses = await _businessService.GetAllAsync();
                return Ok(businesses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 2. TEK İŞLETME DETAYI (SuperAdmin İçin)
        // GET /api/business/{id}
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var business = await _businessService.GetByIdAsync(id);
                return Ok(business);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 3. İŞLETMENİN KENDİ PROFİLİNİ GETİRMESİ (İşletme Paneli İçin)
        // GET /api/business/profile
        // ─────────────────────────────────────────
        [Authorize(Roles = "Business")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var business = await _businessService.GetByIdAsync(businessId);
                return Ok(business);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 4. YENİ İŞLETME OLUŞTUR (Sadece SuperAdmin)
        // POST /api/business
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBusinessDto dto)
        {
            try
            {
                // 1. İşletmeyi kaydet
                var newBusiness = await _businessService.CreateAsync(dto);

                // 2. İşletme oluşur oluşmaz otomatik olarak ilk QR kodunu üret ve kaydet!
                await _qrCodeService.GenerateAsync(newBusiness.Id, newBusiness.Slug);

                // Güncel QR koduyla birlikte işletmenin detayını dön
                var finalDetail = await _businessService.GetByIdAsync(newBusiness.Id);
                return Ok(finalDetail);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 5. İŞLETME BİLGİLERİNİ GÜNCELLE
        // PUT /api/business/{id}
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Business")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateBusinessDto dto)
        {
            try
            {
                // Güvenlik: Eğer giriş yapan bir işletmeyse, başkasının işletmesini güncelleyemez!
                ValidateBusinessOwnership(id);

                var result = await _businessService.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 6. İŞLETMEYİ SİL (Soft Delete - Sadece SuperAdmin)
        // DELETE /api/business/{id}
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                await _businessService.DeleteAsync(id);
                return Ok(new { message = "İşletme başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 7. İŞLETMEYİ DONDUR / AKTİFLEŞTİR (Sadece SuperAdmin)
        // PATCH /api/business/{id}/toggle-active
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive([FromRoute] int id)
        {
            try
            {
                await _businessService.ToggleActiveAsync(id);
                return Ok(new { message = "İşletmenin aktiflik durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 8. LOGO YOLUNU GÜNCELLE
        // PATCH /api/business/{id}/logo
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Business")]
        [HttpPatch("{id}/logo")]
        public async Task<IActionResult> UpdateLogo([FromRoute] int id, [FromBody] UpdateImageRequest request)
        {
            try
            {
                ValidateBusinessOwnership(id);
                await _businessService.UpdateLogoAsync(id, request.ImagePath);
                return Ok(new { message = "Logo başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 9. BANNER YOLUNU GÜNCELLE
        // PATCH /api/business/{id}/banner
        // ─────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Business")]
        [HttpPatch("{id}/banner")]
        public async Task<IActionResult> UpdateBanner([FromRoute] int id, [FromBody] UpdateImageRequest request)
        {
            try
            {
                ValidateBusinessOwnership(id);
                await _businessService.UpdateBannerAsync(id, request.ImagePath);
                return Ok(new { message = "Banner başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 10. İŞLETMENİN KENDİ QR TARAMA İSTATİSTİKLERİ
        // GET /api/business/statistics
        // ─────────────────────────────────────────
        [Authorize(Roles = "Business")]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                int businessId = GetCurrentBusinessId();
                var stats = await _businessService.GetStatisticsAsync(businessId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // GÜVENLİK YARDIMCILARI
        // ─────────────────────────────────────────
        private int GetCurrentBusinessId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "BusinessId");
            if (claim == null || int.TryParse(claim.Value, out int businessId) == false)
            {
                throw new UnauthorizedAccessException("Oturum bilgileriniz doğrulanamadı.");
            }
            return businessId;
        }

        private void ValidateBusinessOwnership(int targetBusinessId)
        {
            // Eğer giriş yapan SuperAdmin ise her işletmeye erişebilir
            if (User.IsInRole("SuperAdmin") == true)
                return;

            // Eğer Business ise sadece kendi BusinessId'sine işlem yapabilir
            int currentBusinessId = GetCurrentBusinessId();
            if (currentBusinessId != targetBusinessId)
            {
                throw new UnauthorizedAccessException("Başka bir işletmenin bilgilerini düzenleyemezsiniz.");
            }
        }
    }

    public class UpdateImageRequest
    {
        public string ImagePath { get; set; } = null!;
    }
}