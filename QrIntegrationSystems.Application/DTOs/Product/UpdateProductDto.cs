using System.ComponentModel.DataAnnotations;
namespace QrIntegrationSystems.Application.DTOs.Product
{
    // Ürün güncellerken kullanılır.
    // ImagePath ayrı upload endpoint'inden yönetilir, buraya dahil değil.
    public class UpdateProductDto
    {
        public int CategoryId { get; set; }
        [Required, StringLength(120), RegularExpression(@"(?s).*\S.*", ErrorMessage = "İsim boş olamaz.")]
        public string Name { get; set; } = null!;
        [StringLength(2000)]
        public string? Description { get; set; }
        [Range(typeof(decimal), "0", "100000000")]
        public decimal Price { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
