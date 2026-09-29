namespace QrIntegrationSystems.Application.DTOs.Business
{
    // SuperAdmin yeni işletme oluştururken bu veriyi gönderir.
    // Slug server tarafında otomatik üretilir.
    // LogoPath ve BannerPath ayrı upload endpoint'inden gelir, buraya dahil değil.
    public class CreateBusinessDto
    {
        public string Name { get; set; } = null!;
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public int TemplateId { get; set; }
    }
}
