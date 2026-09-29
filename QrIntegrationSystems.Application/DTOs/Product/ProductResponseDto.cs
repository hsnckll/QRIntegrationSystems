namespace QrIntegrationSystems.Application.DTOs.Product
{
    // Ürün listesi veya tekil ürün dönerken kullanılır.
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        // Join ile gelen kategori adı — frontend'e ayrıca sorgu yaptırmamak için
        public string CategoryName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
