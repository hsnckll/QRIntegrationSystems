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
        [HttpGet("{slug}")]
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
    }
}