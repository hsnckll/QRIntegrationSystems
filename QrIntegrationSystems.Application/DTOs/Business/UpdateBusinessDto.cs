using System.ComponentModel.DataAnnotations;
namespace QrIntegrationSystems.Application.DTOs.Business
{
    // İşletme bilgilerini güncellerken kullanılır.
    // Slug sabittir. Email gönderilmezse mevcut giriş adresi korunur.
    // LogoPath ve BannerPath ayrı upload endpoint'inden yönetilir.
    public class UpdateBusinessDto
    {
        [Required, StringLength(120), RegularExpression(@"(?s).*\S.*", ErrorMessage = "İsim boş olamaz.")]
        public string Name { get; set; } = null!;
        [EmailAddress, StringLength(254)]
        public string? Email { get; set; }
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int? TemplateId { get; set; }
    }
}
