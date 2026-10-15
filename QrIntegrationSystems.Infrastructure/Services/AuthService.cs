using Microsoft.Extensions.Options;
using QrIntegrationSystems.Infrastructure.Configuration;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QrIntegrationSystems.Application.DTOs.Auth;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Data;
using QrIntegrationSystems.Infrastructure.EmailTemplates;

namespace QrIntegrationSystems.Infrastructure.Services;

public sealed class AuthService(AppDbContext db, IOptions<JwtOptions> options, IEmailService email, AuthSessionStore sessions) : IAuthService
{
    private const int OtpValidityMinutes = 5;
    private readonly JwtOptions jwt = options.Value;
    private static string Normalize(string value) => value.Trim().ToLowerInvariant();
    public async Task<TokenResponseDto> AdminLoginAsync(AdminLoginDto dto, string ipAddress = "unknown")
    {
        var address = Normalize(dto.Email); using var held = await sessions.AcquireAsync("admin:" + address);
        if (Encoding.UTF8.GetByteCount(dto.Password) > 72) throw new ValidationException("Şifre izin verilen uzunluğu aşıyor.");
        await CheckLockout(address, "admin-account"); await CheckLockout("admin-ip", ipAddress);
        var admin = await db.SuperAdmins.FirstOrDefaultAsync(a => a.Email.ToLower() == address);
        if (admin == null || !BCrypt.Net.BCrypt.Verify(dto.Password, admin.Password))
        {
            await Failure("admin-ip", ipAddress); await Failure(address, "admin-account"); await CheckLockout(address, "admin-account"); await CheckLockout("admin-ip", ipAddress);
            throw new ValidationException("E-posta veya şifre hatalı.");
        }
        await Reset(address, "admin-account");
        return Token(admin.Email, "SuperAdmin", null);
    }
    public async Task SendOtpAsync(LoginRequestDto dto, string ipAddress)
    {
        var address = Normalize(dto.Email); using var held = await sessions.AcquireAsync("otp:" + address);
        await CheckLockout(address, "business-account"); await CheckLockout(address, ipAddress);
        var business = await FindOtpBusinessAsync(address);
        if (business == null) throw new ValidationException("Bu e-posta adresiyle işletme girişi yapılamıyor. Kayıtlı işletme e-postanızı kullanın.");
        var now = DateTime.UtcNow;
        var latest = await db.OTPCodes.Where(o => o.Email == address).OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
        if (latest != null && latest.CreatedAt.AddSeconds(30) > now) throw new AuthRateLimitException((int)Math.Ceiling((latest.CreatedAt.AddSeconds(30) - now).TotalSeconds));
        if (await db.OTPCodes.CountAsync(o => o.Email == address && o.CreatedAt > now.AddHours(-1)) >= 10) throw new AuthRateLimitException(3600);
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        // Mail must succeed before a newer challenge replaces existing usable challenges.
        await email.SendAsync(address, "OkutYe giriş doğrulama kodunuz", OtpEmailTemplate.Render(code, OtpValidityMinutes), OtpEmailTemplate.RenderText(code, OtpValidityMinutes));
        foreach (var old in await db.OTPCodes.Where(o => o.Email == address && !o.IsUsed).ToListAsync()) old.IsUsed = true;
        db.OTPCodes.Add(new OTPCode { Email = address, Code = Digest(address, code), CreatedAt = now, ExpiresAt = now.AddMinutes(OtpValidityMinutes), IsUsed = false });
        await db.SaveChangesAsync();
    }
    public async Task<TokenResponseDto> VerifyOtpAsync(OTPVerifyDto dto, string ipAddress)
    {
        var address = Normalize(dto.Email); using var held = await sessions.AcquireAsync("otp:" + address);
        await CheckLockout(address, "business-account"); await CheckLockout(address, ipAddress);
        var record = await db.OTPCodes.Where(o => o.Email == address && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow).OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
        var candidate = Digest(address, dto.Code.Trim());
        if (record == null || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(record.Code), Encoding.UTF8.GetBytes(candidate)))
        {
            if (record != null && ++record.FailedAttempts >= 5) record.IsUsed = true;
            await Failure(address, ipAddress); await Failure(address, "business-account"); await CheckLockout(address, "business-account"); await CheckLockout(address, ipAddress);
            throw new ValidationException("Geçersiz veya süresi dolmuş kod.");
        }
        var business = await FindOtpBusinessAsync(address);
        if (business == null) throw new ValidationException("Geçersiz veya süresi dolmuş kod.");
        record.IsUsed = true; await Reset(address, ipAddress); await Reset(address, "business-account"); await db.SaveChangesAsync();
        return Token(business.Email, "Business", business.Id);
    }
    // Administrator identities must never receive a Business session, even if duplicated in Businesses.
    private Task<Business?> FindOtpBusinessAsync(string address) => db.Businesses.FirstOrDefaultAsync(b =>
        b.Email.ToLower() == address && b.IsActive && !b.IsDeleted &&
        !db.SuperAdmins.Any(a => a.Email.ToLower() == address));

    private string Digest(string address, string code) => "v1:" + Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(jwt.SecretKey), Encoding.UTF8.GetBytes(address + ":" + code)));
    private async Task CheckLockout(string address, string ip)
    {
        var attempt = await db.LoginAttempts.FirstOrDefaultAsync(a => a.Email == address && a.IpAddress == ip);
        if (attempt?.LockedUntil > DateTime.UtcNow) throw new AuthRateLimitException((int)Math.Ceiling((attempt.LockedUntil.Value - DateTime.UtcNow).TotalSeconds));
    }
    private async Task Failure(string address, string ip)
    {
        using var held = await sessions.AcquireAsync("attempt:" + address + "|" + ip);
        var now = DateTime.UtcNow;
        var attempt = await db.LoginAttempts.FirstOrDefaultAsync(a => a.Email == address && a.IpAddress == ip);
        if (attempt == null) { attempt = new LoginAttempt { Email = address, IpAddress = ip }; db.LoginAttempts.Add(attempt); }
        if (attempt.LastAttemptAt < now.AddMinutes(-5) || attempt.LockedUntil <= now) { attempt.FailedCount = 0; attempt.LockedUntil = null; }
        attempt.FailedCount++; attempt.LastAttemptAt = now;
        if (attempt.FailedCount >= 5) attempt.LockedUntil = now.AddMinutes(5);
        await db.SaveChangesAsync();
    }
    private async Task Reset(string address, string ip)
    {
        var attempt = await db.LoginAttempts.FirstOrDefaultAsync(a => a.Email == address && a.IpAddress == ip);
        if (attempt != null) { attempt.FailedCount = 0; attempt.LockedUntil = null; await db.SaveChangesAsync(); }
    }
    private TokenResponseDto Token(string address, string role, int? businessId)
    {
        var expires = DateTime.UtcNow.AddMinutes(Math.Clamp(jwt.ExpirationInMinutes, 5, 1440));
        var claims = new List<Claim> { new(ClaimTypes.Email, address), new(ClaimTypes.Role, role), new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")) };
        if (businessId.HasValue) { claims.Add(new("BusinessId", businessId.Value.ToString())); claims.Add(new("sessionVersion", sessions.BusinessVersion(businessId.Value))); }
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, expires: expires,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)), SecurityAlgorithms.HmacSha256));
        return new TokenResponseDto { Token = new JwtSecurityTokenHandler().WriteToken(token), Role = role, ExpiresAt = expires };
    }
}
