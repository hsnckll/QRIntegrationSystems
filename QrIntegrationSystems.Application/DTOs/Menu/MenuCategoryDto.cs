namespace QrIntegrationSystems.Application.DTOs.Menu
{
    // Public menüde bir kategoriyi ve içindeki ürünleri temsil eder.
    // IsActive=false olan kategoriler zaten filtrelenmiş olarak gelmez.
    public class MenuCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int SortOrder { get; set; }
        public List<MenuProductDto> Products { get; set; } = new();
    }
}
