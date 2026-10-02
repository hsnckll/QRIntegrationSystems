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
                await _authService.SendOtpAsync(dto);
                return Ok(new { message = "Giriş kodu başarıyla e-posta adresinize gönderildi." });
            }
            catch (Exception ex)
            {
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
                var response = await _authService.VerifyOtpAsync(dto);
                return Ok(response); // 200 OK ile JWT Token döner
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}