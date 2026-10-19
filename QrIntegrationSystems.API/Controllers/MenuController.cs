using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Adres: /api/menu
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // ─────────────────────────────────────────
        // QR OKUTULDUĞUNDA AÇILAN PUBLİC MENÜ
        // GET /api/menu/{slug} (Örn: /api/menu/doydoy)
        // ─────────────────────────────────────────
        [HttpGet("{slug}"), EnableRateLimiting("public-menu")]
        public async Task<IActionResult> GetMenuBySlug([FromRoute] string slug)
        {
            // Slug boş mu kontrolü
            if (string.IsNullOrWhiteSpace(slug) == true)
            {
                return BadRequest(new { message = "Geçersiz menü bağlantısı." });
            }

            var menu = await _menuService.GetMenuBySlugAsync(slug);

            // İşletme bulunamadıysa veya sistemden kapatılmışsa (IsActive == false)
            if (menu == null)
            {
                return NotFound(new { message = "İşletme bulunamadı veya menü şu anda yayında değil." });
            }

            // Menü başarıyla bulundu (Arka planda QRScans tablosuna +1 okuma kaydedildi)
            return Ok(menu);
        }

        // ─────────────────────────────────────────
        // CADDY ON-DEMAND TLS DOĞRULAMA UCU
        // GET /api/menu/verify-domain?domain=doydoy-doner.okutye.com.tr
        // ─────────────────────────────────────────
        [HttpGet("verify-domain")]
        public async Task<IActionResult> VerifyDomain([FromQuery] string? domain)
        {
            if (string.IsNullOrWhiteSpace(domain) == true)
            {
                return BadRequest();
            }

            string cleanDomain = domain.Trim().ToLowerInvariant();
            string rootDomain = "okutye.com.tr";

            if (cleanDomain == rootDomain || cleanDomain == $"www.{rootDomain}")
            {
                return Ok();
            }

            string suffix = $".{rootDomain}";
            if (cleanDomain.EndsWith(suffix) == false)
            {
                return NotFound();
            }

            string slug = cleanDomain.Substring(0, cleanDomain.Length - suffix.Length);
            if (string.IsNullOrWhiteSpace(slug) == true || slug.Contains('.') == true)
            {
                return NotFound();
            }

            bool isActive = await _menuService.IsSlugActiveAsync(slug);
            if (isActive == false)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}