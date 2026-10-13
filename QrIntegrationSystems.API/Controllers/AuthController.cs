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
        try { return Ok(await auth.AdminLoginAsync(dto, ClientIp())); }
        catch (AuthRateLimitException ex) { return Limited(ex); }
        catch (Exception ex) { return ApiErrors.FromException(this, ex); }
    }
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp(LoginRequestDto dto)
    {
        try { await auth.SendOtpAsync(dto, ClientIp()); return Ok(new { message = "E-posta kayıtlı ve aktif bir işletmeye aitse doğrulama kodu gönderildi." }); }
        catch (AuthRateLimitException ex) { return Limited(ex); }
        catch (Exception ex) { return ApiErrors.FromException(this, ex); }
    }
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(OTPVerifyDto dto)
    {
        try { return Ok(await auth.VerifyOtpAsync(dto, ClientIp())); }
        catch (AuthRateLimitException ex) { return Limited(ex); }
        catch (Exception ex) { return ApiErrors.FromException(this, ex); }
    }
    [Authorize, HttpPost("logout")]
    public IActionResult Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (jti != null && long.TryParse(User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value, out var exp)) sessions.Revoke(jti, DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime);
        return NoContent();
    }
    private string ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    private IActionResult Limited(AuthRateLimitException ex)
    {
        Response.Headers.RetryAfter = ex.RetryAfterSeconds.ToString();
        return StatusCode(429, new { code = "IP_LOCKED", retryAfterSeconds = ex.RetryAfterSeconds, message = ex.Message });
    }
}
