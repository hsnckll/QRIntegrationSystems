namespace QrIntegrationSystems.Application.Interfaces
{
    // OTP kod üretimi ve doğrulama sorumluluğu burada.
    // AuthService bu interface'i kullanır, doğrudan OTP mantığına bağımlı olmaz.
    public interface IOtpService
    {
        // 6 haneli kod üretir, OTPCodes tablosuna kaydeder, kodu döner
        Task<string> GenerateAndSaveAsync(string email);

        // Kodun geçerli, süresi dolmamış ve kullanılmamış olup olmadığını kontrol eder
        // Başarılıysa kodu "kullanıldı" olarak işaretler
        Task<bool> ValidateAsync(string email, string code);
    }
}
