using System.Globalization;
using System.Net;

namespace QrIntegrationSystems.Infrastructure.EmailTemplates;

public static class OtpEmailTemplate
{
    private const string ResourcePrefix = "QrIntegrationSystems.Infrastructure.EmailTemplates.";
    private static readonly Lazy<string> HtmlTemplate = new(() =>
    {
        using var stream = OpenResource("otp-login.html");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    });

    public static string Render(string code, int validityMinutes = 5)
    {
        Validate(code, validityMinutes);

        return HtmlTemplate.Value
            .Replace("{{OTP_CODE}}", WebUtility.HtmlEncode(code), StringComparison.Ordinal)
            .Replace("{{EXPIRY_MINUTES}}", validityMinutes.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{{YEAR}}", DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    public static string RenderText(string code, int validityMinutes = 5)
    {
        Validate(code, validityMinutes);
        return $"""
            OkutYe — İşletme paneli

            Giriş kodunuz hazır.

            OkutYe işletme paneline giriş yapmak için doğrulama kodunuz: {code}
            Kodunuz {validityMinutes.ToString(CultureInfo.InvariantCulture)} dakika geçerlidir.

            Bu kodu kimseyle paylaşmayın. OkutYe ekibi sizden giriş kodunuzu istemez.
            Bu giriş isteğini siz başlatmadıysanız, bu e-postayı yok sayabilirsiniz.

            Menü cebinde, afiyet masanda.
            Bu e-posta otomatik olarak gönderilmiştir.
            """;
    }

    private static void Validate(string code, int validityMinutes)
    {
        if (code is null || code.Length != 6 || !code.All(char.IsAsciiDigit))
            throw new ArgumentException("Doğrulama kodu altı rakamdan oluşmalı.", nameof(code));
        if (validityMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(validityMinutes));
    }

    private static Stream OpenResource(string name) =>
        typeof(OtpEmailTemplate).Assembly.GetManifestResourceStream(ResourcePrefix + name)
        ?? throw new InvalidOperationException($"E-posta şablonu kaynağı bulunamadı: {name}");
}
