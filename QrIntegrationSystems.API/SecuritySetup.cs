using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using QrIntegrationSystems.Infrastructure.Data;
using QrIntegrationSystems.Infrastructure.Services;

namespace QrIntegrationSystems.API;

public static class SecuritySetup
{
    public static void ConfigureJwt(JwtBearerOptions options, IConfiguration configuration)
    {
        var secret = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT anahtarı gerekli.");
        if (secret.Length < 32) throw new InvalidOperationException("JWT anahtarı en az 32 karakter olmalı.");
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true, ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidateAudience = true, ValidAudience = configuration["JwtSettings:Audience"],
            ValidateLifetime = true, RequireExpirationTime = true, ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents { OnTokenValidated = async context => {
            var services = context.HttpContext.RequestServices;
            if (!await AuthTokenValidation.IsCurrentAsync(context.Principal!, services.GetRequiredService<AppDbContext>(), services.GetRequiredService<AuthSessionStore>())) context.Fail("Oturum artık geçerli değil.");
        }};
    }
    public static IServiceCollection AddQrRateLimits(this IServiceCollection services)
    {
        return services.AddRateLimiter(options => {
            options.RejectionStatusCode = 429;
            options.OnRejected = async (context, cancellation) => {
                var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var delay) ? Math.Max(1, (int)Math.Ceiling(delay.TotalSeconds)) : 60;
                context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
                await context.HttpContext.Response.WriteAsJsonAsync(new { code = "IP_LOCKED", retryAfterSeconds = seconds, message = "Çok fazla istek gönderdiniz. Biraz sonra tekrar deneyin." }, cancellation);
            };
            options.AddPolicy("uploads", context => RateLimitPartition.GetConcurrencyLimiter(context.User.FindFirst("BusinessId")?.Value ?? "admin", _ => new ConcurrencyLimiterOptions { PermitLimit = 2, QueueLimit = 0 }));
            options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 50, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
            options.AddPolicy("public-menu", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
    }
}
