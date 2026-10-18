# QR Platform

Giriş, işletme paneli, SuperAdmin ve müşteri menülerinin tek çalışma alanı. Tek bağımlılık kurulumu, tek Vite sunucusu, aynı origin üzerinde ortak demo oturumu ve ortak demo verisi.

## Başlatma

```sh
npm install
npm run dev
```

`pnpm install` / `pnpm dev` de kullanılabilir; depoda pnpm-lock.yaml bulunur. Kilitli sürümlerle tekrarlanabilir kurulum için `pnpm install --frozen-lockfile` kullanın.

http://127.0.0.1:5177

| Ekran | Adres |
|---|---|
| Giriş seçimi | /index.html |
| Yönetici girişi | /index.html#admin |
| İşletme e-posta + OTP | /index.html#business |
| İşletme paneli | /customer.html |
| SuperAdmin | /superadmin.html |
| Ortak demo verisiyle müşteri menüsü | /menu.html?slug=doy-doy |
| Önceki Doy Doy özel taslağı | /doydoy.html |

Yönetici demo bilgileri: `yonetici@example.com` / `Demo1234`. İşletme: geçerli herhangi bir e-posta + `123456`. İşletme demosu b1/Doy Doy kaydına bağlıdır; farklı e-posta yeni işletme yaratmaz. Gerçek OTP e-postası gönderilmez.

Başarılı giriş aynı sekmede ilgili panele doğrudan yönlenir. Ara başarı ekranı yoktur. Çıkış ortak oturumu temizler, giriş seçimine döner. Yenilemede demo oturumu korunur; süre veya rol uygun değilse panel gösterilmez. Bu kontroller UX içindir; gerçek yetkilendirme backend'de yapılmalıdır.

## Kod düzeni

```text
src/
  entries/                  # Sayfa girişleri; her sayfa kendi CSS'ini yükler
  features/
    auth/                   # Seçim, yönetici girişi, işletme OTP
    customer/               # İşletme paneli, ayrı görünüm dosyaları, repository
    superadmin/             # SuperAdmin, ayrı görünüm dosyaları, repository
  shared/                   # Ortak form, ikon, modal, medya, oturum, hata sınırı
    styles/dashboard.css    # İki panelin tek ortak temel teması
  services/
    auth.js                 # Şu an seçilmiş demo auth adaptörü
    demo/                   # Ortak veri deposu, seed, örnek istatistikler
    api/                    # HTTP istemcisi, gerçek endpoint'ler, DTO dönüşümleri
  templates/doydoy/         # Önceki özgün Doy Doy müşteri taslağı
tests/                      # HTTP/DTO birim testleri ve tarayıcı akış testi
docs/                       # Entegrasyon sözleşmesi, açık backend farkları
archive/                    # Önceki bağımsız projeler; aktif kaynak değildir
```

Çok sayfalı Vite yapısı kullanılır. Form ve CSS çakışmaları önlenir; React ve ortak modüller build sırasında paylaşılan parçalara ayrılır. .html adresleri backend yönlendirme kuralı gerektirmeden statik sunulabilir.

## Veri ve oturum

Aktif ortak demo veri anahtarı `qr-platform-demo-v1`, sekmeye ait oturum anahtarı `qr-platform-session-v1`. İşletme ve SuperAdmin aynı origin'de aynı işletme verisini kullanır. Bir paneldeki değişiklik diğer panel yeniden açıldığında/yenilendiğinde görünür. Açık müşteri menüsü sekmesi storage olaylarını dinler.

Eski portların localStorage alanları ayrı olduğundan önceki demo denemeleri otomatik olarak taşınmaz; eski projeler ve tarayıcı kayıtları silinmedi. Yeni proje örnek veriyle başlar. İstenirse eski veriler açık bir export/import işlemiyle taşınabilir.

`doydoy.html` korunmuş bağımsız tasarım örneğidir ve kendi ürün listesini içerir. Panel verisiyle çalışan canlı demo menü `menu.html` adresidir. Gerçek özel şablon bağlantısı yapılırken Doy Doy bileşenine API menü modeli aktarılmalı; seed içerikler kaldırılmalıdır.

## Backend hazırlığı

- `.env.example`: API/proxy adresleri; hiçbir gizli anahtar içermez.
- `services/api/http.js`: Bearer token, JSON/FormData, timeout/abort, 401 yönlendirmesi, 403/validation hata bilgisi.
- `services/api/endpoints.js`: incelenen .NET controller'larının gerçek adresleri; eksik endpoint uydurulmaz.
- `services/api/mappers.js`: frontend modelleri ile mevcut DTO alanlarını ayırır.
- Feature repository dosyaları: ekranları veri kaynağından ayıran değiştirme noktaları.

**API servisleri hazırlanmıştır; ekranlar henüz gerçek API'ye bağlı değildir.** Demo auth ve demo repository birlikte kullanılıyor. Auth'u tek başına API'ye çevirmek yeterli değildir. Yanlışlıkla API oturumu açılırsa panel demo verisine düşmez; entegrasyonun tamamlanması gerektiğini gösterir. Henüz desteklenmeyen işlemler ve sıralı plan [docs/INTEGRATION-STATUS.md](docs/INTEGRATION-STATUS.md) dosyasındadır.

Geliştirmede `/api` ve `/uploads`, HTTPS localhost:7168 hedefine proxy edilir. `secure:false` yalnızca Vite geliştirme proxy'sinin yerel sertifikası içindir. Üretim sunucusunda `/api` ve `/uploads` için güvenilir HTTPS reverse proxy ayarlanmalı veya izinli API origin kullanılmalıdır. Backend kaynakları/veritabanı bu yeniden düzenlemede değiştirilmedi.

## Doğrulama

```sh
npm run build
npm test
```

`tests/browser.cjs` bu makinedeki Playwright ve Chrome ile çalışır (`node tests/browser.cjs`). Başka makinede Playwright yolunu kurulu pakete göre uyarlayın. `PREVIEW_URL` ile geliştirme/üretim önizleme adresi değiştirilebilir. Testler izole tarayıcı bağlamı kullanır; kişisel demo kayıtlarını değiştirmez.

Arşivdeki projeleri yeni geliştirme için kullanmayın. Arşivleme sonrası eski node_modules bağlantıları geçerli olmayabilir; eski bir taslağı ayrı çalıştırmak gerekirse o klasörde bağımlılıklarını yeniden kurun.
