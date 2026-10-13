using System.ComponentModel.DataAnnotations;
namespace QrIntegrationSystems.Application.DTOs.Product
{
    // Business Admin yeni ürün eklerken bu veriyi gönderir.
    // BusinessId JWT token'dan okunur, bu DTO'ya dahil değil.
    // ImagePath ayrı upload endpoint'inden gelir, buraya dahil değil.
    public class CreateProductDto
    {
        public int CategoryId { get; set; }
        [Required, StringLength(120), RegularExpression(@"(?s).*\S.*", ErrorMessage = "İsim boş olamaz.")]
        public string Name { get; set; } = null!;
        [StringLength(2000)]
        public string? Description { get; set; }
        [Range(typeof(decimal), "0", "100000000")]
        public decimal Price { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}
