using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QrIntegrationSystems.Application.DTOs.Auth;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Infrastructure.Services;

namespace QrIntegrationSystems.API.Controllers;

[ApiController, Route("api/auth"), EnableRateLimiting("auth")]
public sealed class AuthController(IAuthService auth, AuthSessionStore sessions) : ControllerBase
{
    [HttpPost("admin-login")]
    public async Task<IActionResult> AdminLogin(AdminLoginDto dto)
    {
        return Ok(await auth.AdminLoginAsync(dto, ClientIp()));
    }
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp(LoginRequestDto dto)
    {
        await auth.SendOtpAsync(dto, ClientIp());
        return Ok(new { message = "Doğrulama kodu işletme e-posta adresinize gönderildi." });
    }
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(OTPVerifyDto dto)
    {
        return Ok(await auth.VerifyOtpAsync(dto, ClientIp()));
    }
    [Authorize, HttpPost("logout")]
    public IActionResult Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (jti != null && long.TryParse(User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value, out var exp)) sessions.Revoke(jti, DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime);
        return NoContent();
    }
    private string ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

}
