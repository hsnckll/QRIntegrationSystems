namespace QrIntegrationSystems.Application.DTOs.Template
{
    public class TemplateCreateDto
    {
        public string Name { get; set; } = null!;
        public string FolderName { get; set; } = null!;
        public string? ThumbnailPath { get; set; }
    }
}
