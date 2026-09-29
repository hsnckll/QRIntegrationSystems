using QrIntegrationSystems.Application.DTOs.Business;

namespace QrIntegrationSystems.Application.Interfaces
{
    // SuperAdmin'in işletme yönetimi için tüm operasyonlar burada.
    public interface IBusinessService
    {
        Task<List<BusinessListDto>> GetAllAsync();
        Task<BusinessDetailDto> GetByIdAsync(int id);
        Task<BusinessDetailDto> CreateAsync(CreateBusinessDto dto);
        Task<BusinessDetailDto> UpdateAsync(int id, UpdateBusinessDto dto);

        // Soft delete: IsDeleted=true, DeletedAt=now
        Task DeleteAsync(int id);

        // IsActive alanını tersine çevirir (true → false, false → true)
        Task ToggleActiveAsync(int id);

        // Görsel yükleme ayrı endpoint'ten geliyor, sadece path güncelleniyor
        Task UpdateLogoAsync(int id, string logoPath);
        Task UpdateBannerAsync(int id, string bannerPath);
    }
}
