using System.ComponentModel.DataAnnotations;
namespace QrIntegrationSystems.Application.DTOs.Template
{
    public class TemplateCreateDto
    {
        [Required, StringLength(120), RegularExpression(@"(?s).*\S.*", ErrorMessage = "İsim boş olamaz.")]
        public string Name { get; set; } = null!;
        public string FolderName { get; set; } = null!;
        public string? ThumbnailPath { get; set; }
    }
}
