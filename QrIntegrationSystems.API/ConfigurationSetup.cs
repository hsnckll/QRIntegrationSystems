using Microsoft.Extensions.Options;
using QrIntegrationSystems.Infrastructure.Configuration;

namespace QrIntegrationSystems.API;

public static class ConfigurationSetup
{
    public static IServiceCollection AddBackendOptions(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JwtOptions>().Bind(config.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SecretKey) && o.SecretKey.Length >= 32, "JwtSettings:SecretKey en az 32 karakter olmalı.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer) && !string.IsNullOrWhiteSpace(o.Audience), "JwtSettings:Issuer ve Audience gerekli.")
            .Validate(o => o.ExpirationInMinutes is >= 5 and <= 1440, "JwtSettings:ExpirationInMinutes 5–1440 aralığında olmalı.").ValidateOnStart();
        services.AddOptions<SmtpOptions>().Bind(config.GetSection(SmtpOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Server) && o.Port is > 0 and <= 65535 && !string.IsNullOrWhiteSpace(o.SenderEmail) && !string.IsNullOrWhiteSpace(o.Password), "SmtpSettings: Server, geçerli Port, SenderEmail ve Password gerekli.")
            .Validate(o => !o.SaveSentCopy || (!string.IsNullOrWhiteSpace(o.ImapServer) && o.ImapPort is >= 1 and <= 65535), "Gönderilen kopyası için SmtpSettings:ImapServer ve geçerli ImapPort gerekli.")
            .ValidateOnStart();
        services.AddOptions<StorageOptions>().Bind(config.GetSection(StorageOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.AuthPath) && !string.IsNullOrWhiteSpace(o.TemplatesPath) && !string.IsNullOrWhiteSpace(o.UploadsPath), "Storage dizinleri boş olamaz.")
            .Validate(o => o.MaxImageBytes is > 0 and <= 8 * 1024 * 1024 && o.MaxImagePixels is > 0 and <= 16_000_000 && o.BusinessImageQuotaBytes > 0, "Storage görsel sınırları pozitif olmalı; 8 MB ve 16 megapiksel güvenlik sınırları aşılamaz.").ValidateOnStart();
        services.AddOptions<AppOptions>().Bind(config.GetSection(AppOptions.SectionName))
            .Validate(o => Uri.TryCreate(o.MenuBaseUrl.Replace("{slug}", "example"), UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"), "AppSettings:MenuBaseUrl geçerli bir HTTP(S) adresi olmalı.")
            .Validate(o => o.AllowedOrigins.Length > 0 && o.AllowedOrigins.All(x => Uri.TryCreate(x, UriKind.Absolute, out var u) && (u.Scheme == "http" || u.Scheme == "https") && u.AbsolutePath == "/" && string.IsNullOrEmpty(u.Query) && string.IsNullOrEmpty(u.Fragment)), "AppSettings:AllowedOrigins geçerli origin adresleri içermeli.")
            .Validate(o => o.TrustedProxies.All(x => System.Net.IPAddress.TryParse(x, out _)), "AppSettings:TrustedProxies geçerli IP adresleri içermeli.").ValidateOnStart();
        return services;
    }
}
