using QrIntegrationSystems.Application.DTOs.Template;

namespace QrIntegrationSystems.Application.Interfaces
{
    // Template listesi: işletme oluştururken/güncellerken seçim yapılacak.
    // Template yönetimi (ekleme/silme) manuel/DB üzerinden yapılıyor,
    // SuperAdmin panelinde şimdilik CRUD endpoint'i planlanmıyor.
    public interface ITemplateService
    {
        // Tüm template'leri listeler — dropdown için
        Task<List<TemplateResponseDto>> GetAllAsync();
    }
}
