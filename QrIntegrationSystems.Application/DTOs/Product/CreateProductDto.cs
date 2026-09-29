namespace QrIntegrationSystems.Application.DTOs.Product
{
    // Business Admin yeni ürün eklerken bu veriyi gönderir.
    // BusinessId JWT token'dan okunur, bu DTO'ya dahil değil.
    // ImagePath ayrı upload endpoint'inden gelir, buraya dahil değil.
    public class CreateProductDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}
