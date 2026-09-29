namespace QrIntegrationSystems.Application.DTOs.Subscription
{
    // Abonelik bilgisi dönerken kullanılır.
    public class SubscriptionResponseDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? AmountPaid { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }

        // Hesaplanan alan: EndDate > şimdiki zaman mı?
        // DB'de tutulmaz, DTO üretilirken doldurulur.
        public bool IsActive => EndDate > DateTime.UtcNow;
    }
}
