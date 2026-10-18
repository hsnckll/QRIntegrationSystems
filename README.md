# Okutye QR Menu

Backend ve frontend kaynaklari ayni depoda tutulur.

| Klasor | Icerik |
| --- | --- |
| `QrIntegrationSystems.API/` | ASP.NET Core API |
| `QrIntegrationSystems.Application/` | Uygulama katmani |
| `QrIntegrationSystems.Domain/` | Domain modelleri |
| `QrIntegrationSystems.Infrastructure/` | PostgreSQL, migration ve altyapi servisleri |
| `frontend/` | QR Platform Beta V1 React/Vite kaynaklari |

## Kaynaklari sunucuya alma

Sunucunun GitHub erisimi ayarlandiktan sonra:

```bash
mkdir -p "$HOME/apps"
git clone --config core.sshCommand="ssh -i $HOME/.ssh/okutye_backend_git -o IdentitiesOnly=yes" --branch feat_update_beta --single-branch git@github.com:hsnckll/QRIntegrationSystems.git "$HOME/apps/okutye"
```

Sonraki guncellemeler:

```bash
cd "$HOME/apps/okutye"
git pull --ff-only
```

Bu komutlar kaynak kodu alir. Uygulama servislerinin kurulmasi, PostgreSQL baglantisi ve Caddy yayin ayarlari ayrica yapilmalidir.

## Frontend derleme

Node.js 24 kullanilarak dogrulanir. Frontend API istekleri varsayilan olarak ayni origin uzerindeki `/api` adresine gider. Uretimde bu yol backend'e yonlendirilmelidir; Vite gelistirme proxy'si uretim derlemesinde calismaz.

```bash
cd frontend
npm ci
npm test
npm run build
```

Derleme sonucu `frontend/dist/` klasorune yazilir. `node_modules/` ve `dist/` Git'e alinmaz.

## Backend derleme

SDK secimi kokteki `global.json` dosyasindadir; API hedefi `net9.0`'dir.

```bash
dotnet restore QrIntegrationSystems.sln
dotnet publish QrIntegrationSystems.API/QrIntegrationSystems.API.csproj --configuration Release --output ./artifacts/backend
```

Calistirmadan once PostgreSQL, JWT, SMTP ve uygulama adres ayarlari sunucuya ozel yapilandirilmalidir. Veritabani parolalari, ozel SSH anahtarlari ve diger gizli degerler depoya eklenmez. `frontend/.env.example` sadece herkese acik istemci ayarlarinin ornegidir.
