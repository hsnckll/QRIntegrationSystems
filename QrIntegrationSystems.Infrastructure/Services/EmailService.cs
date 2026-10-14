using Microsoft.Extensions.Options;
using QrIntegrationSystems.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpOptions _options;

        public EmailService(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var smtpSettings = _options;
            var server = smtpSettings.Server;

            if (string.IsNullOrWhiteSpace(server)) throw new InvalidOperationException("SMTP sunucusu yapılandırılmalı.");

            // ─────────────────────────────────────────────────────────────
            // CANLI MOD: Hosting bilgileri girildiğinde gerçek e-posta gönderimi
            // ─────────────────────────────────────────────────────────────
            try
            {
                var port = smtpSettings.Port;
                var senderName = smtpSettings.SenderName;
                var senderEmail = (smtpSettings.SenderEmail).Trim();
                var password = (smtpSettings.Password).Replace(" ", "").Trim();

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();

                // 465 portu ise SSL, 587 ise StartTls kullan
                var securityOption = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                await smtp.ConnectAsync(server, port, securityOption);
                await smtp.AuthenticateAsync(senderEmail, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                Console.WriteLine($"[BAŞARILI] {toEmail} adresine e-posta başarıyla iletildi.");
            }
            catch (Exception ex)
            {
                // Mail sunucusunda anlık bir aksilik çıkarsa projeyi çökertme, konsola hata bas
                Console.WriteLine($"[SMTP HATASI]: E-posta gönderilemedi! Hata detayı: {ex.Message}");
                throw new InvalidOperationException("E-posta gönderilemedi. Daha sonra tekrar deneyin.");
            }
        }
    }
}
