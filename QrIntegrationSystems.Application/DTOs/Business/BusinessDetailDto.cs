using QrIntegrationSystems.Application.DTOs.QRCode;
using QrIntegrationSystems.Application.DTOs.Subscription;
using QrIntegrationSystems.Application.DTOs.Template;

namespace QrIntegrationSystems.Application.DTOs.Business
{
    // Tekil işletme detay ekranında dönen tam veri.
    // Aktif abonelik, QR kodu ve template bilgilerini de içerir.
    public class BusinessDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public string? LogoPath { get; set; }
        public string? BannerPath { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // İlişkili veriler
        public TemplateResponseDto? Template { get; set; }
        public QRCodeResponseDto? ActiveQRCode { get; set; }
        public SubscriptionResponseDto? ActiveSubscription { get; set; }
    }
}
