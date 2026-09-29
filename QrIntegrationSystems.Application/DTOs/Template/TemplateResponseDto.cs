namespace QrIntegrationSystems.Application.DTOs.Template
{
    // İşletme oluştururken veya güncellerken template seçim dropdown'ı için kullanılır.
    // Aynı zamanda BusinessDetailDto içinde de gömülü olarak döner.
    public class TemplateResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // Frontend bu değeri okuyarak hangi temayı render edeceğine karar verir
        public string FolderName { get; set; } = null!;
        public string? ThumbnailPath { get; set; }
    }
}
