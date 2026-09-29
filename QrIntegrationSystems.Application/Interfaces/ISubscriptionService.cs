using QrIntegrationSystems.Application.DTOs.Subscription;

namespace QrIntegrationSystems.Application.Interfaces
{
    // SuperAdmin'in işletmelere abonelik eklemesi ve görüntülemesi için.
    public interface ISubscriptionService
    {
        // Bir işletmenin tüm abonelik geçmişi (en yeniden eskiye)
        Task<List<SubscriptionResponseDto>> GetByBusinessIdAsync(int businessId);

        // Yeni abonelik ekle — SuperAdmin manuel olarak abonelik tanımlar
        Task<SubscriptionResponseDto> CreateAsync(int businessId, CreateSubscriptionDto dto);

        // Aboneliği sil (soft delete)
        Task DeleteAsync(int businessId, int id);

        // Bir işletmenin aktif aboneliği var mı?
        // EndDate > DateTime.UtcNow olan en son aboneliği döner, yoksa null
        Task<SubscriptionResponseDto?> GetActiveAsync(int businessId);
    }
}
