using System.ComponentModel.DataAnnotations;
namespace QrIntegrationSystems.Application.DTOs.Category
{
    // Kategori güncellerken kullanılır.
    // IsActive ile kategori geçici olarak pasife alınabilir (ürünler silinmez).
    public class UpdateCategoryDto
    {
        [Required, StringLength(120), RegularExpression(@"(?s).*\S.*", ErrorMessage = "İsim boş olamaz.")]
        public string Name { get; set; } = null!;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
