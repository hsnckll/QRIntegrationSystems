using System.ComponentModel.DataAnnotations;
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
        private readonly TemplatePackageStore _packages;

        public TemplateService(AppDbContext db, TemplatePackageStore packages)
        {
            _db = db;
            _packages = packages;
        }

        // ─────────────────────────────────────────
        // TÜM ŞABLONLARI LİSTELE
        // (Arayüzdeki seçim kutusu / dropdown için)
        // ─────────────────────────────────────────
        public async Task<List<TemplateResponseDto>> GetAllAsync()
        {
            var list = await _db.Templates
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
            foreach (var item in list)
            {
                item.EntryUrl = _packages.EntryUrl(item.FolderName);
                item.Version = _packages.Manifest(item.FolderName)?.Version;
            }
            return list;
        }

        // ─────────────────────────────────────────
        // YENİ ŞABLON OLUŞTUR
        // ─────────────────────────────────────────
        public Task<TemplateResponseDto> CreateAsync(TemplateCreateDto dto) => throw new ValidationException("Şablon oluşturmak için hazırlanmış web paketi yükleyin.");

        // ─────────────────────────────────────────
        // ŞABLON SİL (SuperAdmin)
        // ─────────────────────────────────────────
        public async Task DeleteAsync(int id)
        {
            var inUseCount = await _db.Businesses.CountAsync(b => b.TemplateId == id);
            if (inUseCount > 0)
            {
                throw new ValidationException($"Bu şablon şu anda {inUseCount} işletme tarafından kullanılmaktadır. Silmeden önce ilgili işletmelerin şablonunu değiştirin.");
            }

            var template = await _db.Templates.FindAsync(id);
            if (template == null)
            {
                throw new ValidationException("Silinmek istenen şablon bulunamadı.");
            }

            var packagePath = _packages.DirectoryFor(template.FolderName);
            _db.Templates.Remove(template);
            await _db.SaveChangesAsync();
            if(Directory.Exists(packagePath)) Directory.Delete(packagePath, true);
        }
    }
}
