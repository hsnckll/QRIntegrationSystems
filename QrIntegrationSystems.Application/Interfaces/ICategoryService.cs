using QrIntegrationSystems.Application.DTOs.Category;

namespace QrIntegrationSystems.Application.Interfaces
{
    // Business Admin'in kendi kategorilerini yönetmesi için.
    // businessId her metoda geçiliyor çünkü servis, JWT'den gelen
    // kimliği doğrulamak için bu değere ihtiyaç duyacak.
    public interface ICategoryService
    {
        Task<List<CategoryResponseDto>> GetAllAsync(int businessId);
        Task<CategoryResponseDto> GetByIdAsync(int businessId, int id);
        Task<CategoryResponseDto> CreateAsync(int businessId, CreateCategoryDto dto);
        Task<CategoryResponseDto> UpdateAsync(int businessId, int id, UpdateCategoryDto dto);

        // Soft delete
        Task DeleteAsync(int businessId, int id);
    }
}
