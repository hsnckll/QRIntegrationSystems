namespace QrIntegrationSystems.Application.DTOs.QRCode
{
    // QR kod bilgisi dönerken kullanılır.
    // BusinessDetailDto içinde gömülü olarak kullanılır.
    // QR kodlar sistem tarafından otomatik oluşturulur, kullanıcı tarafından oluşturulmaz.
    public class QRCodeResponseDto
    {
        public int Id { get; set; }

        // QR görselinin dosya yolu — frontend bu URL'i <img> tag'inde kullanır
        public string QRImagePath { get; set; } = null!;

        // QR'ın yönlendirdiği adres (menü URL'i)
        public string TargetUrl { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
