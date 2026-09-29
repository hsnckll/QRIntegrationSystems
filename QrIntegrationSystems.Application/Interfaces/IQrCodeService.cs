namespace QrIntegrationSystems.Application.Interfaces
{
    // QR kod görseli üretmek ve kaydetmekten sorumlu.
    // QRCoder kütüphanesi Infrastructure katmanında kullanılacak.
    public interface IQrCodeService
    {
        // Slug'dan güvenli URL oluşturur, QR görseli üretir,
        // wwwroot/uploads/qrcodes/{businessId}.png olarak kaydeder,
        // kaydedilen dosyanın path'ini döner
        Task<string> GenerateAsync(int businessId, string slug);
    }
}
