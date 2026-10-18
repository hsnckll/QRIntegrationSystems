# Şablonların kullanılabilirliği

Geçerli bir web paketi yüklenir yüklenmez işletme oluşturma ve görünüm düzenleme ekranlarında seçilebilir. Ayrı bir taslak/yayınlama veya “Kullanıma aç” işlemi yoktur. Yükleme mevcut işletmelerin atamasını değiştirmez; müşteri menüsü işletmeye atanan şablonla gösterilir.

`TemplatePackageStore.IsAvailable` dosya sistemindeki v1 manifestini, giriş HTML'ini ve `.capability` bilgisini kontrol eder. Eski `.published` dosyası okunmaz. Dolayısıyla daha önce yüklenmiş taslaklar, dosyaları yerindeyse yeniden yükleme veya veritabanı değişikliği gerektirmeden kullanılabilir. Eksik/bozuk paketler işletmeye atanamaz. SuperAdmin eksik dosyalı kayıtları yönetmek için listede görmeye devam eder; bunların `entryUrl` alanı null olur.

`POST /api/template/package` yüklemeyi doğrular, kurar ve kayıt oluşturur. `GET /api/template` kullanılabilir paketleri işletmelere döner. Oluşturma/güncelleme servisleri ve `GET /api/template/runtime/{code}` aynı dosya kontrolünü kullanır. `isPublished` alanı ve `POST /api/template/{id}/publish` kaldırılmıştır. Frontend ve backend aynı sürümle çalıştırılmalıdır; eski frontend yeni API ile kullanılmamalıdır.

ZIP/yol/tür/boyut denetimleri, SuperAdmin yükleme yetkisi, sandbox ve MessageChannel bağlantısı devam eder. Önizleme tasarımı incelemek içindir; işletmeye atama için önizlemeyi açma veya onaylama şartı yoktur.

Regresyon testi (canlı veritabanına bağlanmaz; SQLite bellek veritabanı ve geçici paket dosyaları kullanır):

```powershell
dotnet test tests/QrIntegrationSystems.TemplateTests/QrIntegrationSystems.TemplateTests.csproj
```

Testler ZIP ve klasör yüklemesini, doğrudan listelenme/atama/menü gösterimini, önceki taslakların uyumluluğunu, atanmamış işletmenin değişmemesini, işletme görünümünü güncellemeyi, kullanılan şablonun silinememesini ve eksik/geçersiz paketlerin reddini kapsar.
