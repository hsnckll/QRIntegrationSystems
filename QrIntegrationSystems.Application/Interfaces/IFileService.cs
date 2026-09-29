namespace QrIntegrationSystems.Application.Interfaces
{
    // Dosya yükleme ve silme sorumluluğu burada.
    // IFormFile ASP.NET Core'a ait olduğu için Application katmanına dahil edilmedi.
    // Controller IFormFile'ı Stream'e çevirip bu servisi çağıracak.
    public interface IFileService
    {
        // fileStream   : dosyanın içeriği
        // fileName     : orijinal dosya adı (uzantı kontrolü için kullanılır)
        // subFolder    : "businesses/1/products" gibi alt klasör yolu
        //
        // Dönen değer  : "/uploads/businesses/1/products/uuid.jpg" gibi
        //                DB'ye kaydedilecek göreli path
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string subFolder);

        // filePath: DB'de kayıtlı göreli path ("/uploads/...")
        Task DeleteAsync(string filePath);
    }
}
