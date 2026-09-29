using QrIntegrationSystems.Application.DTOs.Menu;

namespace QrIntegrationSystems.Application.Interfaces
{
    // Public menü için tek endpoint — QR okutulduğunda çağrılır.
    // Token gerektirmez, slug ile işletme bulunur.
    public interface IMenuService
    {
        // slug: "kafe-istanbul" gibi URL-safe işletme tanımlayıcısı
        // İşletme aktif değilse veya bulunamazsa null döner → controller 404 verir
        Task<PublicMenuDto?> GetMenuBySlugAsync(string slug);
    }
}
