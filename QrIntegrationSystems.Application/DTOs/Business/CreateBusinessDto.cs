using System.ComponentModel.DataAnnotations;
namespace QrIntegrationSystems.Application.DTOs.Business
{
    // SuperAdmin yeni işletme oluştururken bu veriyi gönderir.
    // Slug server tarafında otomatik üretilir.
    // LogoPath ve BannerPath ayrı upload endpoint'inden gelir, buraya dahil değil.
    public class CreateBusinessDto
    {
        [Required, StringLength(120), RegularExpression(@"(?s).*\S.*", ErrorMessage = "İsim boş olamaz.")]
        public string Name { get; set; } = null!;
        [Required, StringLength(80), RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug küçük harf, rakam ve tire içermeli.")]
        public string Slug { get; set; } = null!;  // <-- Siz kendiniz gireceksiniz (örn: "doydoy", "cafe-istanbul")
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public int? TemplateId { get; set; }

        // Başlangıç Abonelik Bilgileri
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public int? SubscriptionMonths { get; set; }
        public decimal? SubscriptionAmountPaid { get; set; }
        public string? SubscriptionPaymentMethod { get; set; }
        public string? SubscriptionNote { get; set; }
    }
}
