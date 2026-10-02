using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");
            var server = smtpSettings["Server"];

            // ─────────────────────────────────────────────────────────────
            // AKILLI MOD: Eğer henüz hosting/mail bilgileri girilmemişse
            // sistemi kilitleme, konsola yazdırarak geliştirmeye izin ver!
            // ─────────────────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(server) == true || server == "mail.qrmenu.com")
            {
                Console.WriteLine($"\n========================================================");
                Console.WriteLine($"[SMTP BEKLEMEDE / GELİŞTİRME MODU]");
                Console.WriteLine($"Kime: {toEmail}");
                Console.WriteLine($"Konu: {subject}");
                Console.WriteLine($"İçerik (HTML): {body}");
                Console.WriteLine($"NOT: Hosting aldığınızda appsettings.json'a bilgileri girince bu mesaj doğrudan e-postaya gidecektir.");
                Console.WriteLine($"========================================================\n");
                return;
            }

            // ─────────────────────────────────────────────────────────────
            // CANLI MOD: Hosting bilgileri girildiğinde gerçek e-posta gönderimi
            // ─────────────────────────────────────────────────────────────
            try
            {
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var senderName = smtpSettings["SenderName"] ?? "QR Menü";
                var senderEmail = smtpSettings["SenderEmail"]!;
                var password = smtpSettings["Password"]!;

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
                Console.WriteLine($"[YEDEK KONSOL MESAJI] Kime: {toEmail} | İçerik: {body}");
            }
        }
    }
}
