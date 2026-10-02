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

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<TokenResponseDto> AdminLoginAsync(AdminLoginDto dto)
        {
            var admin = await _db.SuperAdmins.FirstOrDefaultAsync(a => a.Email == dto.Email);
            // Önce null kontrolü, sonra şifre doğrulaması — sıra önemli
            if (admin == null || !BCrypt.Net.BCrypt.Verify(dto.Password, admin.Password))
                throw new Exception("E-posta veya şifre hatalı.");

            return GenerateToken(admin.Email, "SuperAdmin", null);
        }

        public async Task SendOtpAsync(LoginRequestDto dto)
        {
            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Email == dto.Email && !b.IsDeleted && b.IsActive);

            if (business == null) throw new Exception("Aktif bir işletme bulunamadı.");

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

            Console.WriteLine($"\n========================================");
            Console.WriteLine($"[TEST-EMAIL] {dto.Email} adresi için kodunuz: {code}");
            Console.WriteLine($"========================================\n");
        }

        public async Task<TokenResponseDto> VerifyOtpAsync(OTPVerifyDto dto)
        {
            const int MaxAttempts = 5;

            // 1. Kodu değil, e-postaya ait aktif kaydı bul
            var otpRecord = await _db.OTPCodes
                .Where(o => o.Email == dto.Email
                         && o.IsUsed == false
                         && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
                throw new Exception("Geçersiz veya süresi dolmuş kod.");

            // 2. Kod yanlışsa sayacı artır
            if (otpRecord.Code != dto.Code)
            {
                otpRecord.FailedAttempts++;

                if (otpRecord.FailedAttempts >= MaxAttempts)
                    otpRecord.IsUsed = true; // 5. yanlışta kodu iptal et

                await _db.SaveChangesAsync();
                throw new Exception("Geçersiz veya süresi dolmuş kod."); // Sonrasında buradan tekrar login sayfasına yönlendireceğiz kullanıcıyı
            }

            // 3. Kod doğru: kullanıldı olarak işaretle
            otpRecord.IsUsed = true;
            await _db.SaveChangesAsync();

            // 4. İşletme hâlâ aktif mi kontrol et
            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Email == dto.Email && b.IsActive == true && b.IsDeleted == false);
            if (business == null) throw new Exception("İşletme bulunamadı.");

            return GenerateToken(business.Email, "Business", business.Id);
        }

        private TokenResponseDto GenerateToken(string email, string role, int? businessId)
        {
            var secretKey = _config["JwtSettings:SecretKey"]!;
            var issuer = _config["JwtSettings:Issuer"]!;
            var audience = _config["JwtSettings:Audience"]!;
            var expireMinutes = int.Parse(_config["JwtSettings:ExpirationInMinutes"]!);

            // Kartın üzerindeki bilgiler
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
            var expires = DateTime.UtcNow.AddDays(expireMinutes);

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