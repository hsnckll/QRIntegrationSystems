# Backend bakım düzenlemesi

Uygulanan kapsam: tipli yapılandırma, ortak işletme kapsamı ve merkezi hata yönetimi. Kalıcı test projesi eklenmedi.

- JwtOptions, SmtpOptions, StorageOptions ve AppOptions eklendi. Mevcut yapılandırma anahtarları korundu. Servisler IOptions üzerinden okur.
- JWT/SMTP/storage/adres ayarları başlangıçta doğrulanır. Eksik DefaultConnection için açık hata üretilir. User Secrets değiştirilmedi.
- BusinessScopeResolver: Business kullanıcısı her zaman kendi pozitif BusinessId claim'iyle sınırlıdır. SuperAdmin kategori/ürün işlemlerinde açık businessId seçer; yöneticiye ait genel yüklemelerde işletme seçimi isteğe bağlıdır.
- Controller genel try/catch tekrarları kaldırıldı. Middleware 400/403/404/429/500 eşlemesini yönetir; message, correlationId ve IP_LOCKED/retryAfterSeconds sözleşmeleri korundu. Şablon yüklemedeki temizleme ve pakete özel doğrulama catch/finally blokları korundu.
- Yeni KeyNotFoundException eşlemesi 404 destekler; mevcut servislerin ValidationException ile bildirdiği kayıt hataları uyumluluk için 400 kalır.
- 74 izole kontrol geçti: mevcut 65 kontrol ve yeni 9 kapsam/yapılandırma kontrolü. SQLite bellek veritabanı ve fake SMTP kullanıldı; gerçek Supabase ve SMTP kullanılmadı.

## Çalıştırma notu

Backend yeniden başlatılmalı. JWT Issuer/Audience, SMTP Server/Port/SenderEmail/Password ve DefaultConnection eksikse sunucu başlangıçta açıklayıcı hata ile durur. Mevcut User Secrets içerikleri korunmalı. Yeni migration yoktur. Gerçek PostgreSQL, SMTP ve Linux staging doğrulaması bu yerel testin kapsamı dışındadır.

Önerilen commit: refactor(backend): centralize configuration, business scope and error handling
