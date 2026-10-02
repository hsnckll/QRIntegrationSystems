using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.DTOs.Menu;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _db;

        public MenuService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PublicMenuDto?> GetMenuBySlugAsync(string slug)
        {
            // 1. ADIM: Gelen slug'ı temizle (küçük harf yap ve boşlukları at)
            string cleanSlug = slug.Trim().ToLower();

            // 2. ADIM: Bu slug'a ait aktif ve silinmemiş işletmeyi bul
            // Include(b => b.Template) -> İşletmenin seçtiği menü temasını da getir (modern, klasik vs.)
            // Include(b => b.QRCodes)  -> İstatistiğe kaydetmek için QR kod bilgisini de getir
            var business = await _db.Businesses
                .Include(b => b.Template)
                .Include(b => b.QRCodes)
                .AsNoTracking() // Performans artışı: Değişiklik takibi yapma, sadece oku
                .FirstOrDefaultAsync(b => b.Slug == cleanSlug
                                       && b.IsActive == true
                                       && b.IsDeleted == false);

            // Eğer işletme yoksa veya kapalıysa geriye null dön (Controller 404 verecek)
            if (business == null)
            {
                return null;
            }

            // 3. ADIM: İşletmenin kategorilerini ve o kategorilerin içindeki ürünleri çek
            // Sadece IsActive == true ve IsDeleted == false olanlar gelecek!
            var categories = await _db.Categories
                .Where(c => c.BusinessId == business.Id
                         && c.IsActive == true
                         && c.IsDeleted == false)
                .OrderBy(c => c.SortOrder) // Kategorileri işletmenin belirlediği sıraya göre diz
                .Select(c => new MenuCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    SortOrder = c.SortOrder,
                    // Her kategorinin altındaki ürünleri de filtreleyip sıralayarak DTO'ya dönüştür:
                    Products = c.Products
                        .Where(p => p.IsActive == true && p.IsDeleted == false)
                        .OrderBy(p => p.SortOrder) // Ürünleri belirlenen sıraya göre diz
                        .Select(p => new MenuProductDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            Price = p.Price,
                            ImagePath = p.ImagePath,
                            SortOrder = p.SortOrder
                        })
                        .ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            // 4. ADIM: QR İstatistiğini Kaydet (+1 Tarama)
            // İşletmenin aktif QR kodunu bulalım
            var activeQr = business.QRCodes
                .FirstOrDefault(q => q.IsActive == true && q.IsDeleted == false);

            if (activeQr != null)
            {
                var scanRecord = new QRScan
                {
                    BusinessId = business.Id,
                    QRCodeId = activeQr.Id,
                    ScannedAt = DateTime.UtcNow // Tarama anının UTC zamanı
                };

                // AsNoTracking yapmadığımız yeni bir context kaydı olarak ekliyoruz
                await _db.QRScans.AddAsync(scanRecord);
                await _db.SaveChangesAsync();
            }

            // 5. ADIM: Müşterinin telefonuna gidecek tertemiz PublicMenuDto paketini oluştur ve dön
            return new PublicMenuDto
            {
                BusinessName = business.Name,
                LogoPath = business.LogoPath,
                BannerPath = business.BannerPath,
                // Frontend bu TemplateName değerine bakarak (Örn: "template-modern") hangi tasarımı çizeceğini anlar
                TemplateName = business.Template != null ? business.Template.FolderName : "template-modern",
                Categories = categories
            };
        }
    }
}