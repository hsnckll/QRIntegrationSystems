using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.DTOs.Template;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly AppDbContext _db;

        public TemplateService(AppDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────
        // TÜM ŞABLONLARI LİSTELE
        // (Arayüzdeki seçim kutusu / dropdown için)
        // ─────────────────────────────────────────
        public async Task<List<TemplateResponseDto>> GetAllAsync()
        {
            return await _db.Templates
                .AsNoTracking() // Sadece okuma yapıldığı için takip mekanizmasını kapatıp performans kazanıyoruz
                .OrderBy(t => t.Id)
                .Select(t => new TemplateResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,                   // Görünen isim (Örn: "Modern Tema")
                    FolderName = t.FolderName,       // Dosya/Frontend tema adı (Örn: "template-modern")
                    ThumbnailPath = t.ThumbnailPath  // Önizleme görseli yolu
                })
                .ToListAsync();
        }
    }
}