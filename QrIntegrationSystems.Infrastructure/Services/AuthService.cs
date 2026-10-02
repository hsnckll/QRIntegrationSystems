using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QrIntegrationSystems.Application.DTOs.Auth;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext db, IConfiguration config, IEmailService emailService)
        {
            _db = db;
            _config = config;
            _emailService = emailService;
        }

        public async Task<TokenResponseDto> AdminLoginAsync(AdminLoginDto dto)
        {
            var admin = await _db.SuperAdmins.FirstOrDefaultAsync(a => a.Email == dto.Email);
            
            if (admin == null || BCrypt.Net.BCrypt.Verify(dto.Password, admin.Password) == false)
                throw new Exception("E-posta veya şifre hatalı.");

            return GenerateToken(admin.Email, "SuperAdmin", null);
        }

        public async Task SendOtpAsync(LoginRequestDto dto)
        {
            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Email == dto.Email && b.IsDeleted == false && b.IsActive == true);

            if (business == null)
                throw new Exception("Aktif bir işletme bulunamadı.");

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            var otp = new OTPCode
            {
                Email = dto.Email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _db.OTPCodes.AddAsync(otp);
            await _db.SaveChangesAsync();

            // E-posta gönderimi (HTML formatında şık tasarım)
            string emailSubject = "QR Menü Giriş Doğrulama Kodunuz";
            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px;'>
                    <h2 style='color: #2c3e50;'>QR Menü İşletme Girişi</h2>
                    <p>Yönetim panelinize giriş yapmak için kullanacağınız tek kullanımlık kod:</p>
                    <div style='font-size: 32px; font-weight: bold; color: #27ae60; letter-spacing: 5px; padding: 15px 0;'>
                        {code}
                    </div>
                    <p style='color: #7f8c8d; font-size: 13px;'>Bu kod <strong>5 dakika</strong> süreyle geçerlidir. Eğer bu işlemi siz yapmadıysanız bu e-postayı dikkate almayın.</p>
                </div>";

            // EmailService üzerinden gönder (Hosting ayarı yoksa konsola yazar, varsa gerçek mail atar)
            await _emailService.SendAsync(dto.Email, emailSubject, emailBody);
        }

        public async Task<TokenResponseDto> VerifyOtpAsync(OTPVerifyDto dto)
        {
            const int MaxAttempts = 5;

            var otpRecord = await _db.OTPCodes
                .Where(o => o.Email == dto.Email
                         && o.IsUsed == false
                         && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
                throw new Exception("Geçersiz veya süresi dolmuş kod.");

            if (otpRecord.Code != dto.Code)
            {
                otpRecord.FailedAttempts++;

                if (otpRecord.FailedAttempts >= MaxAttempts)
                    otpRecord.IsUsed = true; // 5. yanlış denemede kodu iptal et

                await _db.SaveChangesAsync();
                throw new Exception("Geçersiz veya süresi dolmuş kod.");
            }

            otpRecord.IsUsed = true;
            await _db.SaveChangesAsync();

            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Email == dto.Email && b.IsActive == true && b.IsDeleted == false);
            if (business == null)
                throw new Exception("İşletme bulunamadı.");

            return GenerateToken(business.Email, "Business", business.Id);
        }

        private TokenResponseDto GenerateToken(string email, string role, int? businessId)
        {
            var secretKey = _config["JwtSettings:SecretKey"]!;
            var issuer = _config["JwtSettings:Issuer"]!;
            var audience = _config["JwtSettings:Audience"]!;
            var expireDays = int.Parse(_config["JwtSettings:ExpirationInDays"] ?? "7");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            if (businessId.HasValue)
            {
                claims.Add(new Claim("BusinessId", businessId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(expireDays);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new TokenResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Role = role,
                ExpiresAt = expires
            };
        }
    }
}