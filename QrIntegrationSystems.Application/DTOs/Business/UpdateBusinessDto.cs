using System.ComponentModel.DataAnnotations;
namespace QrIntegrationSystems.Application.DTOs.Business
{
    // İşletme bilgilerini güncellerken kullanılır.
    // Email ve Slug güvenlik gerekçesiyle bu DTO üzerinden değiştirilemez.
    // LogoPath ve BannerPath ayrı upload endpoint'inden yönetilir.
    public class UpdateBusinessDto
    {
        [Required, StringLength(120), RegularExpression(@"(?s).*\S.*", ErrorMessage = "İsim boş olamaz.")]
        public string Name { get; set; } = null!;
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int? TemplateId { get; set; }
    }
}
