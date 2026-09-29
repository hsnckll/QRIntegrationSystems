using QrIntegrationSystems.Application.DTOs.Product;

namespace QrIntegrationSystems.Application.Interfaces
{
    // Business Admin'in kendi ürünlerini yönetmesi için.
    public interface IProductService
    {
        Task<List<ProductResponseDto>> GetAllAsync(int businessId);
        Task<ProductResponseDto> GetByIdAsync(int businessId, int id);
        Task<ProductResponseDto> CreateAsync(int businessId, CreateProductDto dto);
        Task<ProductResponseDto> UpdateAsync(int businessId, int id, UpdateProductDto dto);

        // Soft delete
        Task DeleteAsync(int businessId, int id);

        // IsActive alanını tersine çevirir — ürünü menüde göster/gizle
        Task ToggleActiveAsync(int businessId, int id);

        // Görsel yükleme ayrı endpoint'ten geliyor, sadece path güncelleniyor
        Task UpdateImageAsync(int businessId, int id, string imagePath);
    }
}
