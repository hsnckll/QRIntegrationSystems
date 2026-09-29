namespace QrIntegrationSystems.Application.Interfaces
{
    // SMTP üzerinden email göndermekten sorumlu tek interface.
    // MailKit implementasyonu Infrastructure katmanında olacak.
    // Application katmanı "nasıl gönderildiğini" bilmez, sadece "gönder" der.
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string body);
    }
}
