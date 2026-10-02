using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.DTOs.Subscription;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _db;

        public SubscriptionService(AppDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────
        // 1. İŞLETMENİN TÜM ABONELİK GEÇMİŞİNİ GETİR
        // (En yeniden en eskiye doğru listeler)
        // ─────────────────────────────────────────
        public async Task<List<SubscriptionResponseDto>> GetByBusinessIdAsync(int businessId)
        {
            return await _db.Subscriptions
                .Where(s => s.BusinessId == businessId && s.IsDeleted == false)
                .OrderByDescending(s => s.StartDate)
                .Select(s => new SubscriptionResponseDto
                {
                    Id = s.Id,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    AmountPaid = s.AmountPaid,
                    PaymentMethod = s.PaymentMethod,
                    Note = s.Note,
                    CreatedAt = s.CreatedAt
                    // IsActive alanı DTO içinde EndDate > Now kontrolüyle otomatik hesaplanır
                })
                .ToListAsync();
        }

        // ─────────────────────────────────────────
        // 2. YENİ ABONELİK TANIMLA (SuperAdmin Ekler)
        // ─────────────────────────────────────────
        public async Task<SubscriptionResponseDto> CreateAsync(int businessId, CreateSubscriptionDto dto)
        {
            // İşletme var mı kontrolü
            var businessExists = await _db.Businesses
                .AnyAsync(b => b.Id == businessId && b.IsDeleted == false);

            if (businessExists == false)
                throw new Exception("Abonelik eklenecek işletme bulunamadı.");

            // Tarih mantık kontrolü: Bitiş tarihi başlangıçtan önce olamaz!
            if (dto.EndDate <= dto.StartDate)
                throw new Exception("Abonelik bitiş tarihi, başlangıç tarihinden sonraki bir tarih olmalıdır.");

            var newSubscription = new Subscription
            {
                BusinessId = businessId,
                StartDate = dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.ToUniversalTime(),
                AmountPaid = dto.AmountPaid,
                PaymentMethod = dto.PaymentMethod?.Trim(),
                Note = dto.Note?.Trim(),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Subscriptions.AddAsync(newSubscription);
            await _db.SaveChangesAsync();

            return new SubscriptionResponseDto
            {
                Id = newSubscription.Id,
                StartDate = newSubscription.StartDate,
                EndDate = newSubscription.EndDate,
                AmountPaid = newSubscription.AmountPaid,
                PaymentMethod = newSubscription.PaymentMethod,
                Note = newSubscription.Note,
                CreatedAt = newSubscription.CreatedAt
            };
        }

        // ─────────────────────────────────────────
        // 3. ABONELİK SİL (Soft Delete)
        // ─────────────────────────────────────────
        public async Task DeleteAsync(int businessId, int id)
        {
            var subscription = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id && s.BusinessId == businessId && s.IsDeleted == false);

            if (subscription == null)
                throw new Exception("Silinecek abonelik kaydı bulunamadı.");

            subscription.IsDeleted = true;
            subscription.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // 4. İŞLETMENİN GÜNCEL/AKTİF ABONELİĞİNİ GETİR
        // (Süresi henüz dolmamış olan en son abonelik)
        // ─────────────────────────────────────────
        public async Task<SubscriptionResponseDto?> GetActiveAsync(int businessId)
        {
            var activeSub = await _db.Subscriptions
                .Where(s => s.BusinessId == businessId
                         && s.IsDeleted == false
                         && s.EndDate > DateTime.UtcNow) // Bitiş tarihi bugünden sonra olanlar
                .OrderByDescending(s => s.EndDate)        // En ileri tarihlisini al
                .FirstOrDefaultAsync();

            if (activeSub == null)
                return null; // Aktif aboneliği yok veya süresi dolmuş

            return new SubscriptionResponseDto
            {
                Id = activeSub.Id,
                StartDate = activeSub.StartDate,
                EndDate = activeSub.EndDate,
                AmountPaid = activeSub.AmountPaid,
                PaymentMethod = activeSub.PaymentMethod,
                Note = activeSub.Note,
                CreatedAt = activeSub.CreatedAt
            };
        }
    }
}