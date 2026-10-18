# Gönderilen e-postaların Hostinger'da görünmesi

Site SMTP ile e-posta gönderdikten sonra, aynı mesajı IMAP üzerinden gönderen hesabın gönderilmiş posta klasörüne kaydeder. Bu özellik `SmtpSettings:SaveSentCopy` ile açılır; projedeki `appsettings.json` içinde açıktır.

Hostinger için ayarlar:

```json
{
  "SmtpSettings": {
    "SaveSentCopy": true,
    "ImapServer": "imap.hostinger.com",
    "ImapPort": 993
  }
}
```

IMAP bağlantısında mevcut SMTP gönderen adresi ve parolası kullanılır. Bağlantı şifrelidir ve sunucu sertifikası normal biçimde doğrulanır. Sunucu gönderilmiş posta klasörünü bildiriyorsa o klasör kullanılır; aksi durumda mevcut `Sent`, `INBOX.Sent`, `Sent Items` ve `Sent Messages` klasörleri denenir. Farklı bir ad gerekiyorsa `SmtpSettings:SentFolder` ile tam IMAP klasör yolu belirtilebilir. Yeni klasör oluşturulmaz.

Gönderim sunucusu mesajı kabul etmediyse kopya kaydedilmez ve mevcut gönderim hatası döndürülür. Kabul ettikten sonraki bağlantı kapatma veya IMAP hataları OTP gönderimini başarısız saymaz. IMAP işlemi için toplam 10 saniye süre sınırı vardır. Kaydedilen mesaj okunmuş olarak işaretlenir.

Konsol kayıtları:

- `[BAŞARILI]`: SMTP sunucusu gönderimi kabul etti; bu tek başına alıcının gelen kutusuna teslim garantisi değildir.
- `[GÖNDERİLEN KOPYASI]`: Gönderilmiş posta klasörüne kopya kaydedildi.
- `[GÖNDERİLEN KOPYASI UYARISI]`: SMTP gönderimi başarılı, ancak kopya kaydedilemedi.

Değişikliklerin canlı sistemde çalışması için backend yeniden yayımlanmalıdır. Canlı ortamda SMTP ayarları ortam değişkenleriyle geçersiz kılınıyorsa, karşılık gelen `SmtpSettings__SaveSentCopy`, `SmtpSettings__ImapServer`, `SmtpSettings__ImapPort` ve gerekirse `SmtpSettings__SentFolder` değerleri de kontrol edilmelidir. Sunucudan IMAP hizmetine çıkış izni ve posta hesabında IMAP erişimi gerekir.

Bu özellik etkinleştirildikten sonra gönderilen yeni mesajları arşivler; geçmişte gönderilen mesajları geriye dönük getirmez. Hostinger webmail'de `SenderEmail` ile aynı posta kutusunun açılması gerekir. DNS kurulum uyarısı ayrı olarak değerlendirilir.
