using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.DTOs.Business;
using QrIntegrationSystems.Application.DTOs.QRCode;
using QrIntegrationSystems.Application.DTOs.Subscription;
using QrIntegrationSystems.Application.DTOs.Template;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly AppDbContext _db;

        public BusinessService(AppDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────
        // 1. TÜM İŞLETMELERİ LİSTELE (SuperAdmin için özet liste)
        // ─────────────────────────────────────────
        public async Task<List<BusinessListDto>> GetAllAsync()
        {
            return await _db.Businesses
                .Where(b => b.IsDeleted == false)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BusinessListDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Slug = b.Slug,
                    Email = b.Email,
                    OwnerName = b.OwnerName,
                    Phone = b.Phone,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
        }

        // ─────────────────────────────────────────
        // 2. TEK İŞLETME DETAYI (Template, Aktif QR ve Abonelik ile)
        // ─────────────────────────────────────────
        public async Task<BusinessDetailDto> GetByIdAsync(int id)
        {
            var business = await _db.Businesses
                .Include(b => b.Template)
                .Include(b => b.QRCodes)
                .Include(b => b.Subscriptions)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (business == null)
                throw new Exception("İşletme bulunamadı.");

            return MapToDetailDto(business);
        }

        // ─────────────────────────────────────────
        // 3. YENİ İŞLETME OLUŞTUR (Slug'ı siz belirlersiniz)
        // ─────────────────────────────────────────
        public async Task<BusinessDetailDto> CreateAsync(CreateBusinessDto dto)
        {
            // Slug temizleme (küçük harfe çevir ve boşlukları temizle)
            string cleanSlug = dto.Slug.Trim().ToLower();

            // Slug daha önce alınmış mı kontrolü
            bool slugExists = await _db.Businesses
                .AnyAsync(b => b.Slug == cleanSlug && b.IsDeleted == false);

            if (slugExists == true)
                throw new Exception("Bu slug (subdomain) zaten kullanılıyor. Lütfen başka bir slug belirleyin.");

            // Email kontrolü
            bool emailExists = await _db.Businesses
                .AnyAsync(b => b.Email == dto.Email && b.IsDeleted == false);

            if (emailExists == true)
                throw new Exception("Bu e-posta adresi zaten kayıtlı.");

            // Template kontrolü
            bool templateExists = await _db.Templates
                .AnyAsync(t => t.Id == dto.TemplateId);

            if (templateExists == false)
                throw new Exception("Seçilen şablon bulunamadı.");

            var newBusiness = new Business
            {
                Name = dto.Name,
                Slug = cleanSlug, // Sizin belirlediğiniz slug
                Email = dto.Email,
                OwnerName = dto.OwnerName,
                Phone = dto.Phone,
                Address = dto.Address,
                TemplateId = dto.TemplateId,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Businesses.AddAsync(newBusiness);
            await _db.SaveChangesAsync();

            // ─────────────────────────────────────────
            // Otomatik Başlangıç Aboneliği Oluştur
            // ─────────────────────────────────────────
            var subStart = dto.SubscriptionStartDate ?? DateTime.UtcNow;
            DateTime subEnd;
            if (dto.SubscriptionEndDate.HasValue == true)
            {
                subEnd = dto.SubscriptionEndDate.Value;
            }
            else
            {
                int months = (dto.SubscriptionMonths.HasValue == true && dto.SubscriptionMonths.Value > 0)
                    ? dto.SubscriptionMonths.Value
                    : 3; // Varsayılan 3 ay başlangıç paketi
                subEnd = subStart.AddMonths(months);
            }

            var initialSubscription = new Subscription
            {
                BusinessId = newBusiness.Id,
                StartDate = subStart,
                EndDate = subEnd,
                AmountPaid = dto.SubscriptionAmountPaid ?? 0,
                PaymentMethod = string.IsNullOrWhiteSpace(dto.SubscriptionPaymentMethod) == false
                    ? dto.SubscriptionPaymentMethod
                    : "Ücretsiz / Deneme",
                Note = string.IsNullOrWhiteSpace(dto.SubscriptionNote) == false
                    ? dto.SubscriptionNote
                    : "İşletme başlangıç paketi",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _db.Subscriptions.AddAsync(initialSubscription);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(newBusiness.Id);
        }

        // ─────────────────────────────────────────
        // 4. İŞLETME BİLGİLERİNİ GÜNCELLE
        // ─────────────────────────────────────────
        public async Task<BusinessDetailDto> UpdateAsync(int id, UpdateBusinessDto dto)
        {
            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (business == null)
                throw new Exception("İşletme bulunamadı.");

            // Template kontrolü
            bool templateExists = await _db.Templates
                .AnyAsync(t => t.Id == dto.TemplateId);

            if (templateExists == false)
                throw new Exception("Seçilen şablon bulunamadı.");

            business.Name = dto.Name;
            business.OwnerName = dto.OwnerName;
            business.Phone = dto.Phone;
            business.Address = dto.Address;
            business.TemplateId = dto.TemplateId;
            business.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        // ─────────────────────────────────────────
        // 5. İŞLETMEYİ SİL (Soft Delete)
        // ─────────────────────────────────────────
        public async Task DeleteAsync(int id)
        {
            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (business == null)
                throw new Exception("Silinecek işletme bulunamadı.");

            business.IsDeleted = true;
            business.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // 6. AKTİF / PASİF DURUMUNU TERSİNE ÇEVİR
        // ─────────────────────────────────────────
        public async Task ToggleActiveAsync(int id)
        {
            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (business == null)
                throw new Exception("İşletme bulunamadı.");

            business.IsActive = (business.IsActive == false);
            business.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // 7. LOGO YOLUNU GÜNCELLE
        // ─────────────────────────────────────────
        public async Task UpdateLogoAsync(int id, string logoPath)
        {
            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (business == null)
                throw new Exception("İşletme bulunamadı.");

            business.LogoPath = logoPath;
            business.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // 8. BANNER YOLUNU GÜNCELLE
        // ─────────────────────────────────────────
        public async Task UpdateBannerAsync(int id, string bannerPath)
        {
            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (business == null)
                throw new Exception("İşletme bulunamadı.");

            business.BannerPath = bannerPath;
            business.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // 9. İŞLETMENİN KENDİ QR TARAMA İSTATİSTİKLERİ
        // ─────────────────────────────────────────
        public async Task<BusinessStatisticsDto> GetStatisticsAsync(int businessId)
        {
            var now = DateTime.UtcNow;
            var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var scans = await _db.QRScans
                .AsNoTracking()
                .Where(s => s.BusinessId == businessId)
                .OrderByDescending(s => s.ScannedAt)
                .Select(s => s.ScannedAt)
                .ToListAsync();

            return new BusinessStatisticsDto
            {
                TotalScans = scans.Count,
                TodayScans = scans.Count(s => s >= todayStart),
                ThisMonthScans = scans.Count(s => s >= thisMonthStart),
                Scans = scans
            };
        }

        // ─────────────────────────────────────────
        // YARDIMCI METOT: Detay DTO Dönüşümü
        // ─────────────────────────────────────────
        private BusinessDetailDto MapToDetailDto(Business b)
        {
            var activeQr = b.QRCodes
                .Where(q => q.IsActive == true && q.IsDeleted == false)
                .OrderByDescending(q => q.CreatedAt)
                .FirstOrDefault();

            var activeSub = b.Subscriptions
                .Where(s => s.IsDeleted == false && s.EndDate > DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();

            return new BusinessDetailDto
            {
                Id = b.Id,
                Name = b.Name,
                Slug = b.Slug,
                Email = b.Email,
                OwnerName = b.OwnerName,
                Phone = b.Phone,
                Address = b.Address,
                LogoPath = b.LogoPath,
                BannerPath = b.BannerPath,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                Template = b.Template == null ? null : new TemplateResponseDto
                {
                    Id = b.Template.Id,
                    Name = b.Template.Name,
                    FolderName = b.Template.FolderName,
                    ThumbnailPath = b.Template.ThumbnailPath
                },
                ActiveQRCode = activeQr == null ? null : new QRCodeResponseDto
                {
                    Id = activeQr.Id,
                    QRImagePath = activeQr.QRImagePath,
                    TargetUrl = activeQr.TargetUrl,
                    IsActive = activeQr.IsActive,
                    CreatedAt = activeQr.CreatedAt
                },
                ActiveSubscription = activeSub == null ? null : new SubscriptionResponseDto
                {
                    Id = activeSub.Id,
                    StartDate = activeSub.StartDate,
                    EndDate = activeSub.EndDate,
                    AmountPaid = activeSub.AmountPaid,
                    PaymentMethod = activeSub.PaymentMethod,
                    Note = activeSub.Note,
                    CreatedAt = activeSub.CreatedAt
                }
            };
        }
    }
}