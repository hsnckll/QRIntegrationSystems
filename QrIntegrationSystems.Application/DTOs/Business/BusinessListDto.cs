namespace QrIntegrationSystems.Application.DTOs.Business
{
    // SuperAdmin tüm işletmeleri listelerken dönen özet veri.
    // Ağır alanlar (Logo, Banner, QR, Abonelik) bu DTO'ya dahil değil.
    public class BusinessListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
