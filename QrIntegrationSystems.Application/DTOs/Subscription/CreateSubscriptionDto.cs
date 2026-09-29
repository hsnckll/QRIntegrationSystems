namespace QrIntegrationSystems.Application.DTOs.Subscription
{
    // SuperAdmin bir işletmeye abonelik eklerken bu veriyi gönderir.
    // BusinessId URL parametresinden okunur ({businessId}), bu DTO'ya dahil değil.
    public class CreateSubscriptionDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? AmountPaid { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Note { get; set; }
    }
}
