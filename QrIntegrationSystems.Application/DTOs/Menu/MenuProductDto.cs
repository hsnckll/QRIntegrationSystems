namespace QrIntegrationSystems.Application.DTOs.Menu
{
    // Public menüde bir ürünü temsil eder.
    // IsActive=false olan ürünler zaten filtrelenmiş olarak gelmez.
    // Yönetimsel alanlar (BusinessId, SortOrder, IsDeleted vb.) son kullanıcıya gösterilmez.
    public class MenuProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public int SortOrder { get; set; }
    }
}
