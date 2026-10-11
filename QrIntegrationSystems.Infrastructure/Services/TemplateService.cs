using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.DTOs.Template;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
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

        // ─────────────────────────────────────────
        // YENİ ŞABLON OLUŞTUR
        // ─────────────────────────────────────────
        public async Task<TemplateResponseDto> CreateAsync(TemplateCreateDto dto)
        {
            var cleanFolder = dto.FolderName.Trim().ToLower();

            var exists = await _db.Templates.AnyAsync(t => t.FolderName == cleanFolder);
            if (exists == true)
            {
                throw new Exception("Bu kod adına sahip bir şablon zaten mevcut.");
            }

            var template = new Template
            {
                Name = dto.Name.Trim(),
                FolderName = cleanFolder,
                ThumbnailPath = dto.ThumbnailPath,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Templates.AddAsync(template);
            await _db.SaveChangesAsync();

            return new TemplateResponseDto
            {
                Id = template.Id,
                Name = template.Name,
                FolderName = template.FolderName,
                ThumbnailPath = template.ThumbnailPath
            };
        }

        // ─────────────────────────────────────────
        // ŞABLON SİL (SuperAdmin)
        // ─────────────────────────────────────────
        public async Task DeleteAsync(int id)
        {
            var inUseCount = await _db.Businesses.CountAsync(b => b.TemplateId == id && b.IsDeleted == false);
            if (inUseCount > 0)
            {
                throw new Exception($"Bu şablon şu anda {inUseCount} aktif işletme tarafından kullanılmaktadır. Silmeden önce ilgili işletmelerin şablonunu değiştirin.");
            }

            var template = await _db.Templates.FindAsync(id);
            if (template == null)
            {
                throw new Exception("Silinmek istenen şablon bulunamadı.");
            }

            _db.Templates.Remove(template);
            await _db.SaveChangesAsync();
        }
    }
}