using Microsoft.Extensions.Options;
using QrIntegrationSystems.Infrastructure.Configuration;
using QrIntegrationSystems.Infrastructure.EmailTemplates;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
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

        public async Task SendAsync(string toEmail, string subject, string body, string? textBody = null)
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
                var password = smtpSettings.Password;

                using var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body,
                    TextBody = textBody
                };
                using var logoStream = body.Contains($"cid:{OtpEmailTemplate.LogoContentId}", StringComparison.Ordinal)
                    ? OtpEmailTemplate.OpenLogo()
                    : null;
                if (logoStream is not null)
                {
                    var logo = bodyBuilder.LinkedResources.Add("okutye-logo.png", logoStream, new ContentType("image", "png"));
                    logo.ContentId = OtpEmailTemplate.LogoContentId;
                    logo.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
                }
                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = CreateSmtpClient();

                // 465 portu ise SSL, 587 ise StartTls kullan
                var securityOption = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                await smtp.ConnectAsync(server, port, securityOption);
                await smtp.AuthenticateAsync(senderEmail, password);
                await smtp.SendAsync(email);
                try
                {
                    await smtp.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SMTP KAPATMA UYARISI]: E-posta kabul edildi ancak bağlantı düzgün kapatılamadı: {ex.Message}");
                }

                Console.WriteLine($"[BAŞARILI] {toEmail} için e-posta SMTP sunucusu tarafından kabul edildi.");

                if (smtpSettings.SaveSentCopy)
                    await TrySaveSentCopyAsync(email, senderEmail, password);
            }
            catch (Exception ex)
            {
                // Mail sunucusunda anlık bir aksilik çıkarsa projeyi çökertme, konsola hata bas
                Console.WriteLine($"[SMTP HATASI]: E-posta gönderilemedi! Hata detayı: {ex.Message}");
                throw new InvalidOperationException("E-posta gönderilemedi. Daha sonra tekrar deneyin.", ex);
            }
        }

        protected virtual ISmtpClient CreateSmtpClient() => new SmtpClient();

        protected virtual IImapClient CreateImapClient() => new ImapClient();

        private async Task TrySaveSentCopyAsync(MimeMessage email, string senderEmail, string password)
        {
            // Arşivleme sorunu, SMTP'nin kabul ettiği OTP gönderimini başarısız saymamalı.
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = timeout.Token;

            try
            {
                using var imap = CreateImapClient();
                imap.Timeout = 10_000;
                var securityOption = _options.ImapPort == 993
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await imap.ConnectAsync(_options.ImapServer, _options.ImapPort, securityOption, cancellationToken);
                await imap.AuthenticateAsync(senderEmail, password, cancellationToken);

                var sentFolder = await GetSentFolderAsync(imap, cancellationToken);
                await sentFolder.AppendAsync(email, MessageFlags.Seen, cancellationToken);
                Console.WriteLine("[GÖNDERİLEN KOPYASI]: E-posta gönderilmiş posta klasörüne kaydedildi.");

                try
                {
                    await imap.DisconnectAsync(true, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[IMAP KAPATMA UYARISI]: Kopya kaydedildi ancak bağlantı düzgün kapatılamadı: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GÖNDERİLEN KOPYASI UYARISI]: E-posta gönderildi ancak kopyası kaydedilemedi: {ex.Message}");
            }
        }

        private async Task<IMailFolder> GetSentFolderAsync(IImapClient imap, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_options.SentFolder))
                return await imap.GetFolderAsync(_options.SentFolder, cancellationToken);

            try
            {
                var specialFolder = imap.GetFolder(SpecialFolder.Sent);
                if (specialFolder is not null)
                    return specialFolder;
            }
            catch (NotSupportedException)
            {
                // Bazı sunucular özel klasör bilgisini yayımlamaz; mevcut adları dene.
            }

            foreach (var folderName in new[] { "Sent", "INBOX.Sent", "Sent Items", "Sent Messages" })
            {
                try
                {
                    var folder = await imap.GetFolderAsync(folderName, cancellationToken);
                    if (folder.Exists)
                        return folder;
                }
                catch (FolderNotFoundException)
                {
                    // Klasör oluşturma; sıradaki mevcut klasör adını dene.
                }
            }

            throw new InvalidOperationException("Gönderilmiş posta klasörü bulunamadı. SmtpSettings:SentFolder ayarını belirtin.");
        }
    }
}
