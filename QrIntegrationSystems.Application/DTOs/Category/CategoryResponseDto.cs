namespace QrIntegrationSystems.Application.DTOs.Category
{
    // Kategori listesi veya tekil kategori dönerken kullanılır.
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Kategoriye ait ürün sayısı — listede özet bilgi için
        public int ProductCount { get; set; }
    }
}
