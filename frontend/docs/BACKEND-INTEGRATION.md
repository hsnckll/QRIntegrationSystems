# Mevcut .NET backend ile bağlantı planı

İncelenen kaynak: `C:/Users/ebk90/source/repos/QRIntegrationSystems`. Bu çalışma kaynak kod incelemesidir; API başlatılmadı, Supabase veritabanına erişilmedi ve canlı kayıt değiştirilmedi. Program.cs başlangıçta veritabanına seed yazabildiği için salt inceleme sırasında sunucu çalıştırılmadı.

## Mimari

SuperAdmin React + Customer React → HTTPS .NET Web API → EF Core/Npgsql → Supabase PostgreSQL.

GitHub kod deposudur, API'nin çalıştığı sunucu değildir. Supabase veritabanı bağlantı dizesi ve JWT gizli anahtarı backend ortamında kalır; React VITE değişkenlerine veya tarayıcı koduna konmaz. Panel yalnızca API adresini ve giriş sonucu aldığı oturumu kullanır.

launchSettings.json: HTTPS `https://localhost:7168`, HTTP `http://localhost:5231`. Swagger geliştirme profilinde `/swagger` altında. HTTPS yönlendirme etkin. Bunlar yapılandırılmış adreslerdir; çalışan servis doğrulanmadı.

## Doğrulanmış mevcut endpoint'ler

| İşlem | Endpoint | Not |
|---|---|---|
| SuperAdmin giriş | POST /api/auth/admin-login | Email, Password; yanıt Token, Role, ExpiresAt |
| İşletme giriş | POST /api/auth/send-otp, POST /api/auth/verify-otp | E-posta OTP |
| İşletme listesi/detay | GET /api/business, GET /api/business/{id} | SuperAdmin |
| İşletme ekle | POST /api/business | Otomatik QR üretimi de yapılıyor |
| İşletme güncelle | PUT /api/business/{id} | Name, OwnerName, Phone, Address, TemplateId |
| Sil / aktiflik | DELETE /api/business/{id}, PATCH /api/business/{id}/toggle-active | Soft delete var; restore endpoint yok |
| Logo/banner | PATCH /api/business/{id}/logo veya /banner | Gövde: { imagePath } |
| Dosya yükle | POST /api/upload/image | multipart/form-data: File, Folder; yanıt { url } |
| Şablon listesi | GET /api/template | FolderName kullanılabilir |
| Kategori / ürün | /api/category, /api/product | Mevcut controller yalnızca Business rolüne açık |
| Abonelik | GET /api/subscription/business/{businessId}, GET .../active, POST .../{businessId} | Güncelleme endpoint'i yok; silme mevcut |
| Özet | GET /api/dashboard/summary | SuperAdmin, 7 günlük seri mevcut |
| Müşteri menüsü | GET /api/menu/{slug} | TemplateName, BusinessName, LogoPath, BannerPath, Categories |

## Bağlamadan önce giderilecek farklar

1. Category/Product controller'ları Business rolünü ve token'daki BusinessId'yi kullanıyor. SuperAdmin için açık businessId kapsamlı endpoint veya role göre güvenli çözüm eklenmeli. Yalnızca rol niteliğine SuperAdmin eklemek yeterli değil; mevcut GetCurrentBusinessId akışı da ayrılmalı. İşletme rolü hiçbir zaman başka işletmenin id'sini seçememeli.
2. Logo/banner endpoint'leri şu an SuperAdmin ve Business'a açık. İstenen kurala göre sadece SuperAdmin olmalı. UpdateBusinessDto içindeki TemplateId de Business rolünden değiştirilebiliyor; ayrı DTO/yetki gerekir.
3. UpdateBusinessDto isteğe bağlı `email` kabul eder; gönderilmezse adres korunur. E-posta hem SuperAdmin hem de işletmenin kendi profili üzerinden değiştirilebilir. Kullanılan işletme/yönetici adresleri reddedilir; değişiklik eski işletme oturumlarını ve bekleyen kodları geçersizleştirir. Slug değiştirilemez.
4. İşletme geri yükleme ve silinenleri listeleme endpoint'leri yok. Demo butonları API eklenmeden bağlanmamalı.
5. SubscriptionController'da güncelleme/uzatma yok. Tarih uzatmayı PUT/PATCH ile uygulamak veya yeni dönem kaydı oluşturmak kararlaştırılmalı. Mevcut abonelik iş kurallarıyla çakışan tarih aralıkları kontrol edilmeli.
6. Logs tablosu ve DTO var; controller listesinde log endpoint'i yok. İncelenen servislerde audit yazımı görülmedi. Güvenilir actor, old/new, zaman ve businessId sunucuda, mümkünse işlemle aynı transaction içinde yazılmalı. Tarayıcıdan gelen actor'a güvenilmemeli.
7. QrCodeService QRCoder ile PNG üretip wwwroot/uploads/qrcodes içine kaydediyor. Business oluşturma sırasında çağrılıyor. Panelin tekrar üretme/listeleme/aktiflik talepleri için ayrı endpoint'ler gerekiyor. Canlıda frontend kütüphanesiyle ikinci kayıt üretmek yerine mevcut backend QR servisi kullanılabilir.
8. Mevcut QR hedefi AppSettings:MenuBaseUrl içindeki {slug} değerinden üretiliyor. Yeni QR eski aktif kayıtları pasife alıyor, fakat URL aynı slug'a gidebiliyor. Kod bazlı iptal/ölçüm gerekiyorsa ayrı sabit /q/{publicToken} adresi tasarlanmalı; bu token erişim anahtarı değil, genel QR kimliğidir.
9. MenuService her GET menü isteğinde aktif QR'a QRScan ekliyor. Önizleme, yeniden deneme, doğrudan link ve sayfa yenileme de sayıyı artırır. Ayrı QR yönlendirme olayı veya açık ölçüm endpoint'i kullanılıp API veri okumasından ayrılmalı. Fiziksel okutma ile link ziyaretinin kusursuz ayrımı mümkün değildir; metriğin adı ve kuralları buna göre seçilmeli.
10. DashboardSummary 7 günlük UTC seri döndürüyor. 30 gün/işletme filtresi ve Türkiye saat dilimine göre gruplama isteniyorsa backend genişletilmeli.
11. Ürün/kategori sırası API DTO'larındaki SortOrder alanlarına eşlenmeli. Demo dizisinin sıra değişimini backend'de atomik toplu güncelleme yapmak daha tutarlıdır.

## CORS

Şu an yalnızca `http://localhost:5173` ve `http://localhost:3000` izinli. Bu projelerin varsayılanları `http://127.0.0.1:5175` (müşteri) ve `http://127.0.0.1:5176` (SuperAdmin). Doğrudan API erişiminde kullanılan gerçek origin'ler AllowReactApp'e eklenmeli. localhost ile 127.0.0.1 farklı origin'dir. Alternatif olarak Vite geliştirme proxy'si kullanılabilir. Üretimde yalnızca gerçek panel alan adları tanımlanmalı.

## 100 farklı işletme tasarımı

Mevcut PublicMenuDto.TemplateName zaten Template.FolderName değerini taşıyor. Backend aynı sözleşmeyle veri sunar; React yerel bir güvenli kayıt listesinden uygun bileşeni seçer:

```jsx
const templates = {
  'doy-doy': lazy(() => import('./templates/DoyDoy')),
  'kiyi': lazy(() => import('./templates/Kiyi')),
  'cekirdek': lazy(() => import('./templates/Cekirdek')),
};
const Menu = templates[menu.templateName] ?? DefaultMenu;
// Suspense ve hata sınırı içinde <Menu menu={menu} />
```

100 ayrı tasarım yapılabilir; bütün tasarımları her ziyaretçiye yüklemek gerekmez. Aynı API veri modelini alan ayrı bileşenler olarak derlenir. Ortak ürün/fiyat/kategori mantığı paylaşılır; görsel yerleşim özelleşir. Yönetim panelinden yüklenen rastgele HTML/JS dosyası çalıştırılmaz. Yeni kod tabanlı şablon eklemek test ve dağıtım gerektirir; yalnızca TemplateId seçmek yeni bir tasarım üretmez.

## Entegrasyon sırası

1. Backend test ortamı ve Swagger sözleşmesini doğrula; kullanıcı/işletme yetkilerini düzelt.
2. İki panelde gerçek giriş, token süresi/401/403 yönetimi ve API servis katmanı kur.
3. Önce liste/detay ve şablon okumasını, sonra yazma işlemlerini bağla. API başarısızken demo veriyle sessizce devam etme.
4. Görselleri FormData ile yükle; dönen dosya yolunu business/product kaydına yaz. Veritabanına base64 fotoğraf koyma. Üretim depolama/CDN kararını mevcut FileService ile birlikte uygula.
5. Özel müşteri menüsünü GET /api/menu/{slug} yanıtına bağla; QR ölçümünü ayır.
6. Gerçek işletmelere geçmeden tenant izolasyonu, yetki, silme, süre dolması, QR ve log akışlarını test verileriyle doğrula.

Bu projede bu endpoint'lere otomatik istek gönderilmez; demo ve canlı veriler karıştırılmaz.
