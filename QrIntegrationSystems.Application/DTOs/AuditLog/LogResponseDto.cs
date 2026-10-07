namespace QrIntegrationSystems.Application.DTOs.AuditLog
{
    // SuperAdmin bir işletmenin aktivite geçmişini görürken kullanılır.
    // OldValues ve NewValues JSON string olarak saklanır, frontend parse eder.
    public class LogResponseDto
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }

        // "SuperAdmin" veya "Business"
        public string ActorType { get; set; } = null!;

        // "Ürün güncellendi", "Kategori eklendi" gibi açıklayıcı metin
        public string Action { get; set; } = null!;

        // "Product", "Category" gibi hangi tabloda işlem yapıldığı
        public string EntityType { get; set; } = null!;

        // İşlem yapılan kaydın Id'si
        public int EntityId { get; set; }

        // Değişiklik öncesi değerler (JSON string)
        public string? OldValues { get; set; }

        // Değişiklik sonrası değerler (JSON string)
        public string? NewValues { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
