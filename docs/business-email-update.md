# İşletme giriş e-postasını değiştirme

SuperAdmin işletmenin Bilgiler sekmesinden, işletme ise Bilgilerim ekranından e-posta adresini değiştirebilir. Frontend kaynakları depo içindeki `frontend/` klasöründedir.

`PUT /api/business/{id}` artık isteğe bağlı `email` kabul eder. Gönderilmezse mevcut adres korunur. Adres boş olamaz; geçerli e-posta biçiminde ve en fazla 254 karakter olmalıdır. Kaydetmeden önce baştaki/sondaki boşluklar kaldırılır ve küçük harfe dönüştürülür. Başka bir silinmemiş işletmenin adresi (pasif işletmeler dahil) veya SuperAdmin adresi kullanılamaz. Mevcut yetki sınırı korunur: işletme yalnız kendi kaydını güncelleyebilir.

Adres gerçekten değiştiğinde eski/yeni adrese ait bekleyen OTP'ler geçersizleştirilir ve bu işletmenin mevcut oturumları kapatılır. SuperAdmin kendi oturumunda kalır. İşletme kendi adresini değiştirmişse kaydın ardından açıklama içeren işletme giriş ekranına yönlendirilir; yeni adrese gelen kodla yeniden giriş yapar. Adreste yalnız büyük/küçük harf veya boşluk farkı varsa oturum sonlandırılmaz. Menü adresi ve QR kod değişmez.

Bu özellik veritabanı şema değişikliği gerektirmez. Frontend ve backend birlikte güncellenmelidir; yalnız frontend alanını açmak yeterli değildir.

Doğrulama:

```powershell
dotnet test tests/QrIntegrationSystems.EmailTests/QrIntegrationSystems.EmailTests.csproj
cd frontend
npm test
npm run build
```

Backend testleri izole SQLite bellek veritabanı, geçici oturum dosyaları ve sahte e-posta göndericisi kullanır. Gerçek hesapları değiştirmez veya gerçek e-posta göndermez.
