using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class FileService : IFileService
    {
        // İzin verilen dosya uzantıları (görseller ve şablon paketleri)
        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".zip", ".tsx", ".jsx", ".css", ".json", ".html" };

        // ─────────────────────────────────────────
        // 1. DOSYA / GÖRSEL YÜKLE
        // ─────────────────────────────────────────
        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string subFolder)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new Exception("Yüklenecek dosya bulunamadı veya boş.");

            // Dosya uzantısını al ve küçük harfe çevir (.jpg gibi)
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            // Güvenlik: Uzantı kontrolü
            if (_allowedExtensions.Contains(extension) == false)
                throw new Exception("Geçersiz dosya formatı. İzin verilen formatlar: .jpg, .jpeg, .png, .webp, .zip, .tsx, .jsx, .html.");

            // Fiziksel klasör yolunu belirle: [ProjeKlasörü]/wwwroot/uploads/[subFolder]
            string uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            string targetFolder = Path.Combine(uploadsRoot, subFolder.Trim('/', '\\'));

            // Klasör yoksa oluştur
            if (Directory.Exists(targetFolder) == false)
            {
                Directory.CreateDirectory(targetFolder);
            }

            // Çakışmayı önlemek için benzersiz (GUID) dosya adı üret
            string uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            string fullPhysicalPath = Path.Combine(targetFolder, uniqueFileName);

            // Dosyayı sunucu diskine kaydet
            using (var destinationStream = new FileStream(fullPhysicalPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destinationStream);
            }

            // Web üzerinden erişilecek bağıl (relative) yolu oluştur
            string cleanSubFolder = subFolder.Trim('/', '\\').Replace('\\', '/');
            return $"/uploads/{cleanSubFolder}/{uniqueFileName}";
        }

        // ─────────────────────────────────────────
        // 2. ESKİ GÖRSELİ SİL (Disk Temizliği)
        // ─────────────────────────────────────────
        public Task DeleteAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) == true)
                return Task.CompletedTask;

            // URL formatındaki "/uploads/..." yolunu fiziksel işletim sistemi yoluna dönüştür
            string trimmedPath = filePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", trimmedPath);

            // Dosya gerçekten diskte varsa sil
            if (File.Exists(physicalPath) == true)
            {
                File.Delete(physicalPath);
            }

            return Task.CompletedTask;
        }
    }
}