# Entegrasyon hazırlığı ve kalan işler

## Bu çalışmada tamamlananlar

Tek kök, tek package.json/lockfile, tek sunucu; giriş sonrası doğrudan yönlendirme; ortak demo oturumu ve role uygun sayfa koruması; ortak form/modal bileşenleri; ayrı feature/views dosyaları; ortak işletme verisi; yeniden kullanılabilir HTTP servisi ve mevcut backend DTO eşlemeleri. Demo ağ isteği yapmaz.

API'yi başlatıp veritabanını değiştirmedik. UI'nin gerçek veriyle çalıştığı iddia edilmez. Mevcut DTO/endpoint incelemesi `BACKEND-INTEGRATION.md` içinde korunmuştur; oradaki bağımsız port bilgileri önceki projeye aittir, yeni UI portu 5177'dir.

## API'ye geçiş sırası

1. **Auth:** `services/auth.js` içindeki demo çağrılarını `authApi` ve `sessionFromApi` ile değiştir. BusinessId için backend profil yanıtını kullan. JWT'yi URL'ye yazma. Sunucu doğrulaması olmadan istemci rolünü yetki kaynağı kabul etme.
2. **Feature repository:** `features/customer/repository.js` ve `features/superadmin/repository.js` fonksiyonlarını gerçek entity servislerine dönüştür. Şu anki senkron snapshot API'sini `load` + async entity command yapısına taşı. Ekranlarda await/loading/field-error durumlarını bağla; localStorage snapshot'ını sunucuya topluca gönderme. Başarılı API yanıtını state'e uygula. Hataları demo başarıyla maskeleme.
3. **Dosyalar:** `uploadApi.image` ile FormData yükle; backend'in verdiği url'yi ilgili image/logo/banner endpoint'ine gönder. Fotoğraf base64'ünü DTO'ya göndermeme. Eşzamanlı başarısız yüklemede eski kayıt korunmalı.
4. **Müşteri menüsü:** `menuApi.get(slug)` → güvenli yerel template registry. Özel Doy Doy taslağına ürün/kategori verisini prop olarak aktar; sabit içerikleri çıkar. Sadece kayıtlı şablonları lazy yükle.
5. **Canlı dağıtım:** statik dist, API reverse proxy/izinli CORS, token ömrü ve yetki kontrolleri; ardından tenant izolasyonu ve yazma akışlarını test ortamında doğrula.

Demo oturumunda localStorage yazımına devam edip sadece giriş endpoint'ini bağlamak gerçek entegrasyon değildir. `ProtectedPage` API oturumunu demo panelinden ayırarak bu karışmayı engeller.

## Mevcut backend'de gerekli ekler

- Category/Product controller yalnızca Business rolünü ve BusinessId claim'ini kullanıyor. SuperAdmin için açık, yetkili işletme kapsamı gerekli.
- Logo/banner ve TemplateId değişiklikleri Business rolüne açık; istenen kurala göre SuperAdmin ile sınırlandırılmalı.
- UpdateBusinessDto e-posta değişikliğini kabul eder; slug sabittir. E-posta her iki panelden güncellenebilir. Adres değiştiğinde eski işletme oturumları ve bekleyen giriş kodları geçersizleşir; işletme yeni adresle yeniden giriş yapar.
- Logs okuma ve güvenilir sunucu audit yazımı, abonelik uzatma/güncelleme, işletme restore ve silinenleri listeleme, QR listeleme/yeniden üretme/aktiflik endpoint'leri eksik.
- İşletme paneli abonelik bilgisi için mevcut SuperAdmin-only subscription rotaları yerine güvenli Business scope gerekli.
- Menü GET isteği QRScan ekliyor; preview/data read ile gerçek QR ziyaret ölçümü ayrılmalı.
- Demo UUID kimlikleri sayısal backend Id değildir. API katmanı gerçek sayısal kimlik bekler; demo kayıtlar otomatik canlıya aktarılmaz.

Supabase bağlantı dizesi, service-role anahtarı ve JWT secret hiçbir frontend env dosyasına konmaz. Geliştirme proxy ayarı nedeniyle localhost/127.0.0.1 port farklarını tek origin altında yönetmek mümkündür; üretim için sunucu proxy ayarı ayrıca gerekir.
