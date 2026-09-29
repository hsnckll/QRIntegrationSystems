namespace QrIntegrationSystems.Application.DTOs.Menu
{
    // QR kod okutulduğunda son kullanıcıya dönen ana yanıt.
    // Frontend, TemplateName'e göre hangi tema/bileşeni render edeceğine karar verir.
    public class PublicMenuDto
    {
        public string BusinessName { get; set; } = null!;
        public string? LogoPath { get; set; }
        public string? BannerPath { get; set; }

        // Template.FolderName değeri — frontend bunu okuyarak doğru temayı yükler
        public string TemplateName { get; set; } = null!;

        public List<MenuCategoryDto> Categories { get; set; } = new();
    }
}
