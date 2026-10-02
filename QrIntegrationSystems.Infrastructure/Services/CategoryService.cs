using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.DTOs.Category;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────
        // 1. İŞLETMEYE AİT TÜM KATEGORİLERİ GETİR
        // (Ürün sayılarıyla birlikte sıralı listeler)
        // ─────────────────────────────────────────
        public async Task<List<CategoryResponseDto>> GetAllAsync(int businessId)
        {
            return await _db.Categories
                .Where(c => c.BusinessId == businessId && c.IsDeleted == false)
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    // Kategoriye ait aktif ve silinmemiş ürün sayısı
                    ProductCount = c.Products.Count(p => p.IsDeleted == false)
                })
                .ToListAsync();
        }

        // ─────────────────────────────────────────
        // 2. TEK KATEGORİ DETAYI
        // ─────────────────────────────────────────
        public async Task<CategoryResponseDto> GetByIdAsync(int businessId, int id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == businessId && c.IsDeleted == false);

            if (category == null)
                throw new Exception("Kategori bulunamadı.");

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProductCount = category.Products.Count(p => p.IsDeleted == false)
            };
        }

        // ─────────────────────────────────────────
        // 3. YENİ KATEGORİ EKLE
        // ─────────────────────────────────────────
        public async Task<CategoryResponseDto> CreateAsync(int businessId, CreateCategoryDto dto)
        {
            // İşletme kontrolü
            var businessExists = await _db.Businesses
                .AnyAsync(b => b.Id == businessId && b.IsDeleted == false);

            if (businessExists == false)
                throw new Exception("İşletme bulunamadı.");

            // Aynı isimde kategori var mı kontrolü
            var nameExists = await _db.Categories
                .AnyAsync(c => c.BusinessId == businessId
                            && c.Name.ToLower() == dto.Name.Trim().ToLower()
                            && c.IsDeleted == false);

            if (nameExists == true)
                throw new Exception("Bu isimde bir kategori zaten mevcut.");

            var newCategory = new Category
            {
                BusinessId = businessId,
                Name = dto.Name.Trim(),
                SortOrder = dto.SortOrder,
                IsActive = true,       // Yeni kategori varsayılan olarak aktif
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Categories.AddAsync(newCategory);
            await _db.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = newCategory.Id,
                Name = newCategory.Name,
                SortOrder = newCategory.SortOrder,
                IsActive = newCategory.IsActive,
                CreatedAt = newCategory.CreatedAt,
                UpdatedAt = newCategory.UpdatedAt,
                ProductCount = 0
            };
        }

        // ─────────────────────────────────────────
        // 4. KATEGORİ GÜNCELLE
        // ─────────────────────────────────────────
        public async Task<CategoryResponseDto> UpdateAsync(int businessId, int id, UpdateCategoryDto dto)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == businessId && c.IsDeleted == false);

            if (category == null)
                throw new Exception("Kategori bulunamadı.");

            // Kendi adı dışında aynı isimde başka kategori var mı kontrolü
            var nameExists = await _db.Categories
                .AnyAsync(c => c.BusinessId == businessId
                            && c.Id != id
                            && c.Name.ToLower() == dto.Name.Trim().ToLower()
                            && c.IsDeleted == false);

            if (nameExists == true)
                throw new Exception("Bu isimde başka bir kategori zaten mevcut.");

            category.Name = dto.Name.Trim();
            category.SortOrder = dto.SortOrder;
            category.IsActive = dto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProductCount = category.Products.Count(p => p.IsDeleted == false)
            };
        }

        // ─────────────────────────────────────────
        // 5. KATEGORİ SİL (Soft Delete)
        // ─────────────────────────────────────────
        public async Task DeleteAsync(int businessId, int id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == businessId && c.IsDeleted == false);

            if (category == null)
                throw new Exception("Silinecek kategori bulunamadı.");

            // Kategoriyi sil
            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;

            // Kategori silindiğinde içindeki ürünler de silinmiş (soft delete) işaretlenir
            foreach (var product in category.Products.Where(p => p.IsDeleted == false))
            {
                product.IsDeleted = true;
                product.DeletedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // 6. AKTİF / PASİF DURUMUNU TERSİNE ÇEVİR (Switch Butonu İçin)
        // ─────────────────────────────────────────
        public async Task ToggleActiveAsync(int businessId, int id)
        {
            var category = await _db.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == businessId && c.IsDeleted == false);

            if (category == null)
                throw new Exception("Kategori bulunamadı.");

            // Aktifse pasif, pasifse aktif yapar
            category.IsActive = (category.IsActive == false);
            category.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}