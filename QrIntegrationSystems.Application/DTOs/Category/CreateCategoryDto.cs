namespace QrIntegrationSystems.Application.DTOs.Category
{
    // Business Admin yeni kategori eklerken bu veriyi gönderir.
    // BusinessId JWT token'dan okunur, bu DTO'ya dahil değil.
    public class CreateCategoryDto
    {
        public string Name { get; set; } = null!;

        // Belirtilmezse 0 olarak atanır, sonradan sıralama güncellenebilir.
        public int SortOrder { get; set; } = 0;
    }
}
