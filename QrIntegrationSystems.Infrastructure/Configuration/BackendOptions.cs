namespace QrIntegrationSystems.Infrastructure.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "JwtSettings";
    public string SecretKey { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int ExpirationInMinutes { get; set; } = 60;
}
public sealed class SmtpOptions
{
    public const string SectionName = "SmtpSettings";
    public string Server { get; set; } = "";
    public int Port { get; set; } = 587;
    public string SenderName { get; set; } = "QR Menü";
    public string SenderEmail { get; set; } = "";
    public bool SaveSentCopy { get; set; }
    public string ImapServer { get; set; } = "";
    public int ImapPort { get; set; } = 993;
    public string SentFolder { get; set; } = "";
    public string Password { get; set; } = "";
}
public sealed class StorageOptions
{
    public const string SectionName = "Storage";
    public string AuthPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "storage", "auth");
    public string TemplatesPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "storage", "templates");
    public string UploadsPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    public long MaxImageBytes { get; set; } = 8 * 1024 * 1024;
    public long MaxImagePixels { get; set; } = 16_000_000;
    public long BusinessImageQuotaBytes { get; set; } = 256L * 1024 * 1024;
}
public sealed class AppOptions
{
    public const string SectionName = "AppSettings";
    public string MenuBaseUrl { get; set; } = "https://{slug}.okutye.com.tr";
    public string[] AllowedOrigins { get; set; } = ["http://localhost:5177", "http://127.0.0.1:5177"];
    public string[] TrustedProxies { get; set; } = [];
}
