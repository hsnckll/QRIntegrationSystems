using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Infrastructure.Data;
using QRCoder;
// Çakışmayı önlemek için veritabanı tablomuzu açıkça takma isimle (alias) tanımlıyoruz:
using DbQRCode = QrIntegrationSystems.Domain.Entities.QRCode;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public QrCodeService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ─────────────────────────────────────────
        // SLUG'DAN QR KOD GÖRSELİ ÜRET VE KAYDET
        // ─────────────────────────────────────────
        public async Task<string> GenerateAsync(int businessId, string slug)
        {
            // 1. İşletme var mı kontrol et
            var business = await _db.Businesses
                .FirstOrDefaultAsync(b => b.Id == businessId && b.IsDeleted == false);

            if (business == null)
                throw new Exception("QR kod üretilecek işletme bulunamadı.");

            // 2. Hedef Menü URL'sini belirle
            string baseUrl = _config["AppSettings:MenuBaseUrl"] ?? "https://{slug}.qrmenu.com";
            string targetUrl = baseUrl.Replace("{slug}", slug.Trim().ToLower());

            // 3. QRCoder ile QR verisini oluştur (Yüksek hata düzeltme seviyesi: ECCLevel.Q)
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(targetUrl, QRCodeGenerator.ECCLevel.Q);

            // PngByteQRCode: Saf PNG byte dizisi üretir
            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);

            // 4. Dosyanın kaydedileceği fiziksel yolu belirle
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "qrcodes");

            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"{businessId}_{Guid.NewGuid().ToString().Substring(0, 8)}.png";
            string fullPhysicalPath = Path.Combine(folderPath, fileName);

            // PNG dosyasını diske kaydet
            await File.WriteAllBytesAsync(fullPhysicalPath, qrCodeBytes);

            string relativePath = $"/uploads/qrcodes/{fileName}";

            // 5. Veritabanındaki eski aktif QR kodları pasife al
            var existingQrs = await _db.QRCodes
                .Where(q => q.BusinessId == businessId && q.IsActive == true && q.IsDeleted == false)
                .ToListAsync();

            foreach (var oldQr in existingQrs)
            {
                oldQr.IsActive = false;
                oldQr.UpdatedAt = DateTime.UtcNow;
            }

            // 6. Yeni QR kod kaydını veritabanına ekle (DbQRCode kullanarak çakışmayı önledik)
            var newQrRecord = new DbQRCode
            {
                BusinessId = businessId,
                TargetUrl = targetUrl,
                QRImagePath = relativePath,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _db.QRCodes.AddAsync(newQrRecord);
            await _db.SaveChangesAsync();

            return relativePath;
        }
    }
}