using QrIntegrationSystems.Application.DTOs.Auth;

namespace QrIntegrationSystems.Application.Interfaces
{
    public interface IAuthService
    {
        // SuperAdmin: email + şifre ile giriş → JWT döner
        Task<TokenResponseDto> AdminLoginAsync(AdminLoginDto dto, string ipAddress = "unknown");

        // Business Admin Adım 1: email gönder, OTP kodu üret ve maile gönder
        Task SendOtpAsync(LoginRequestDto dto, string ipAddress);

        // Business Admin Adım 2: OTP kodunu doğrula → JWT döner
        Task<TokenResponseDto> VerifyOtpAsync(OTPVerifyDto dto, string ipAddress);
    }
}
