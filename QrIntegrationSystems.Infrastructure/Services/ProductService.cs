using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.DTOs.Product;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;

        public ProductService(AppDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────
        // 1. İŞLETMENİN TÜM ÜRÜNLERİNİ LİSTELE
        // (Kategori adıyla birlikte sıralı döner)
        // ─────────────────────────────────────────
        public async Task<List<ProductResponseDto>> GetAllAsync(int businessId)
        {
            return await _db.Products
                .Where(p => p.BusinessId == businessId && p.IsDeleted == false)
                .OrderBy(p => p.CategoryId)
                .ThenBy(p => p.SortOrder)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name, // JOIN ile kategori adını alıyoruz
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImagePath = p.ImagePath,
                    SortOrder = p.SortOrder,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();
        }

        // ─────────────────────────────────────────
        // 2. TEK ÜRÜN DETAYI
        // ─────────────────────────────────────────
        public async Task<ProductResponseDto> GetByIdAsync(int businessId, int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.BusinessId == businessId && p.IsDeleted == false);

            if (product == null)
                throw new Exception("Ürün bulunamadı.");

            return MapToResponseDto(product);
        }

        // ─────────────────────────────────────────
        // 3. YENİ ÜRÜN OLUŞTUR
        // ─────────────────────────────────────────
        public async Task<ProductResponseDto> CreateAsync(int businessId, CreateProductDto dto)
        {
            // Fiyat kontrolü
            if (dto.Price < 0)
                throw new Exception("Ürün fiyatı 0'dan küçük olamaz.");

            // Seçilen kategori gerçekten bu işletmeye mi ait kontrolü
            var categoryExists = await _db.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.BusinessId == businessId && c.IsDeleted == false);

            if (categoryExists == false)
                throw new Exception("Geçerli bir kategori bulunamadı veya seçilen kategori işletmenize ait değil.");

            var newProduct = new Product
            {
                BusinessId = businessId,
                CategoryId = dto.CategoryId,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Price = dto.Price,
                SortOrder = dto.SortOrder,
                IsActive = true,       // Yeni ürün varsayılan olarak aktif
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Products.AddAsync(newProduct);
            await _db.SaveChangesAsync();

            // Kategori adı join'li gelebilsin diye GetByIdAsync çağırıyoruz
            return await GetByIdAsync(businessId, newProduct.Id);
        }

        // ─────────────────────────────────────────
        // 4. ÜRÜN GÜNCELLE
        // ─────────────────────────────────────────
        public async Task<ProductResponseDto> UpdateAsync(int businessId, int id, UpdateProductDto dto)
        {
            if (dto.Price < 0)
                throw new Exception("Ürün fiyatı 0'dan küçük olamaz.");

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.BusinessId == businessId && p.IsDeleted == false);

            if (product == null)
                throw new Exception("Güncellenecek ürün bulunamadı.");

            // Güncellenen kategori bu işletmeye mi ait kontrolü
            var categoryExists = await _db.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.BusinessId == businessId && c.IsDeleted == false);

            if (categoryExists == false)
                throw new Exception("Geçerli bir kategori bulunamadı veya seçilen kategori işletmenize ait değil.");

            product.CategoryId = dto.CategoryId;
            product.Name = dto.Name.Trim();
            product.Description = dto.Description?.Trim();
            product.Price = dto.Price;
            product.SortOrder = dto.SortOrder;
            product.IsActive = dto.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await GetByIdAsync(businessId, id);
        }

        // ─────────────────────────────────────────
        // 5. ÜRÜN SİL (Soft Delete)
        // ─────────────────────────────────────────
        public async Task DeleteAsync(int businessId, int id)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.BusinessId == businessId && p.IsDeleted == false);

            if (product == null)
                throw new Exception("Silinecek ürün bulunamadı.");

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // 6. AKTİF / PASİF DURUMUNU TERSİNE ÇEVİR
        // (Menüde hızlıca göster/gizle yapmak için)
        // ─────────────────────────────────────────
        public async Task ToggleActiveAsync(int businessId, int id)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.BusinessId == businessId && p.IsDeleted == false);

            if (product == null)
                throw new Exception("Ürün bulunamadı.");

            product.IsActive = (product.IsActive == false);
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // 7. ÜRÜN GÖRSEL YOLUNU GÜNCELLE
        // ─────────────────────────────────────────
        public async Task UpdateImageAsync(int businessId, int id, string imagePath)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.BusinessId == businessId && p.IsDeleted == false);

            if (product == null)
                throw new Exception("Ürün bulunamadı.");

            product.ImagePath = imagePath;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // YARDIMCI METOT: Detay DTO Dönüşümü
        // ─────────────────────────────────────────
        private ProductResponseDto MapToResponseDto(Product p)
        {
            return new ProductResponseDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImagePath = p.ImagePath,
                SortOrder = p.SortOrder,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }
    }
}