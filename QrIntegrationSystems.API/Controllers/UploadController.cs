using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("uploads")]
    [Authorize] // Yalnızca giriş yapmış kullanıcılar görsel yükleyebilir
    [ApiController]
    [Route("api/[controller]")] // Adres: /api/upload
    public class UploadController : ControllerBase
    {
        private readonly IFileService _fileService;
        private int? Scope(int? selected)
        {
            if (User.IsInRole("SuperAdmin")) return selected;
            if (User.IsInRole("Business") && int.TryParse(User.FindFirst("BusinessId")?.Value, out var id) && id > 0) return id;
            throw new UnauthorizedAccessException();
        }

        public UploadController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // ─────────────────────────────────────────
        // RESİM / GÖRSEL YÜKLE
        // POST /api/upload/image
        // ─────────────────────────────────────────
        [HttpPost("image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request)
        {
            try
            {
                var file = request.File;
                string folder = string.IsNullOrWhiteSpace(request.Folder) == true ? "general" : request.Folder;

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Lütfen geçerli bir görsel dosyası seçin." });
                }

                // IFormFile'ı saf Stream'e çevirip FileService'e teslim ediyoruz
                using var stream = file.OpenReadStream();
                string relativePath = await _fileService.UploadImageAsync(stream, file.FileName, folder, Scope(request.BusinessId), User.IsInRole("SuperAdmin"));

                // Yüklenen dosyanın URL yolunu JSON olarak dönüyoruz
                return Ok(new { url = relativePath });
            }
            catch (Exception ex)
            {
                return ApiErrors.FromException(this, ex);
            }
        }

        // ─────────────────────────────────────────
        // ESKİ GÖRSELİ SİL
        // DELETE /api/upload/image
        // ─────────────────────────────────────────
        [HttpDelete("image")]
        public async Task<IActionResult> DeleteImage([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) == true)
                {
                    return BadRequest(new { message = "Geçersiz dosya yolu." });
                }

                await _fileService.DeleteAsync(filePath, Scope(null), User.IsInRole("SuperAdmin"));
                return Ok(new { message = "Görsel başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return ApiErrors.FromException(this, ex);
            }
        }
    }

    // Swagger ve Form-Data uyumlu dosya yükleme modeli
    public class UploadImageRequest
    {
        public IFormFile File { get; set; } = null!;
        public string? Folder { get; set; } = "general";
        public int? BusinessId { get; set; }
    }
}
