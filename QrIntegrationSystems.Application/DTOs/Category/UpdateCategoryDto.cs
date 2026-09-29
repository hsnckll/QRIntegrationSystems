namespace QrIntegrationSystems.Application.DTOs.Category
{
    // Kategori güncellerken kullanılır.
    // IsActive ile kategori geçici olarak pasife alınabilir (ürünler silinmez).
    public class UpdateCategoryDto
    {
        public string Name { get; set; } = null!;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
