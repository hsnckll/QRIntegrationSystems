namespace QrIntegrationSystems.Application.DTOs.Product
{
    // Ürün güncellerken kullanılır.
    // ImagePath ayrı upload endpoint'inden yönetilir, buraya dahil değil.
    public class UpdateProductDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
