using Microsoft.AspNetCore.Mvc;
using QrIntegrationSystems.Application.DTOs.Auth;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Adres: /api/auth
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Dependency Injection ile AuthService'i çağırıyoruz
        public AuthController(IAuthService _authServiceParam)
        {
            _authService = _authServiceParam;
        }

        // ─────────────────────────────────────────
        // 1. SUPERADMIN GİRİŞİ (Email + Şifre)
        // POST /api/auth/admin-login
        // ─────────────────────────────────────────
        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginDto dto)
        {
            try
            {
                var response = await _authService.AdminLoginAsync(dto);
                return Ok(response); // 200 OK ile JWT Token döner
            }
            catch (Exception ex)
            {
                // Hatalı şifre veya admin bulunamadı durumunda 400 Bad Request döner
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 2. İŞLETME GİRİŞİ ADIM 1: KOD GÖNDER
        // POST /api/auth/send-otp
        // ─────────────────────────────────────────
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] LoginRequestDto dto)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                await _authService.SendOtpAsync(dto, ipAddress);
                return Ok(new { message = "Giriş kodu başarıyla e-posta adresinize gönderildi." });
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("IP_LOCKED:"))
                {
                    var seconds = ex.Message.Split(':')[1];
                    return StatusCode(429, new { code = "IP_LOCKED", retryAfterSeconds = int.Parse(seconds), message = "Çok fazla hatalı deneme yaptınız." });
                }
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────
        // 3. İŞLETME GİRİŞİ ADIM 2: KODU DOĞRULA VE GİRİŞ YAP
        // POST /api/auth/verify-otp
        // ─────────────────────────────────────────
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OTPVerifyDto dto)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                var response = await _authService.VerifyOtpAsync(dto, ipAddress);
                return Ok(response); // 200 OK ile JWT Token döner
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("IP_LOCKED:"))
                {
                    var seconds = ex.Message.Split(':')[1];
                    return StatusCode(429, new { code = "IP_LOCKED", retryAfterSeconds = int.Parse(seconds), message = "Çok fazla hatalı deneme yaptınız." });
                }
                return BadRequest(new { message = ex.Message });
            }
        }

        private string GetClientIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For") == true)
            {
                var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(forwarded) == false)
                {
                    return forwarded.Split(',')[0].Trim();
                }
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}