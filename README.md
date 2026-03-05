# DB Query Explorer

Veritabanı tablolarını görsel olarak keşfetmek, filtrelemek ve sonuçları Excel'e aktarmak için geliştirilmiş masaüstü uygulaması.  
**MySQL** ve **MSSQL** bağlantılarını destekler. .NET 8 tabanlı WPF uygulamasıdır.

---

## Özellikler

- **Çoklu bağlantı profili** — `connections.json` üzerinden önceden tanımlanmış bağlantılar
- **Manuel bağlantı** — sunucu, port, kullanıcı adı ve şifreyi elle girerek bağlanma
- **MySQL ve MSSQL** desteği
- **Tablo listesi** — bağlı veritabanındaki tüm tabloları listeler
- **Sütun seçimi** — hangi sütunların sorguda yer alacağını seçme / tümünü seç / tümünü kaldır
- **Filtreler** — dinamik WHERE koşulları ekleme (çoklu filtre desteği)
- **JOIN desteği** — başka tablolarla görsel join tanımlama
- **Sütun dönüşümleri** — veritabanındaki ham değerleri okunabilir etiketlere dönüştürme (ör. `M → Erkek`, `F → Kadın`)
- **SQL Sorgusu sekmesi** — doğrudan özel SQL yazabilme
- **Satır limiti** — 1K / 5K / 10K / 50K / Tümü seçenekleri
- **Excel'e aktar** — sonuçları `.xlsx` dosyasına aktarma

---

## Gereksinimler

### 1. İşletim Sistemi

| Gereksinim | Detay |
|---|---|
| İşletim Sistemi | Windows 10 veya üstü (64-bit) |
| Mimari | x64 |

---

### 2. .NET 8 Runtime

Uygulama **.NET 8** ile çalışmaktadır. Bilgisayarında .NET kurulu değilse aşağıdaki adımları izle:

#### Adım 1 — .NET kurulu mu kontrol et

`Win + R` → `cmd` → aşağıdaki komutu çalıştır:

```
dotnet --version
```

`8.x.x` çıktısı görürsen kurulum gerekmez.  
Komut tanınmıyorsa ya da sürüm `8`'in altındaysa kurulum gereklidir.

#### Adım 2 — .NET 8 Desktop Runtime indir

> Sadece uygulamayı çalıştırmak için **Runtime** yeterlidir; geliştirme yapacaksan **SDK** indir.

| Amaç | İndir |
|---|---|
| Sadece çalıştırmak | [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) → **Run desktop apps** → **x64** |
| Geliştirme yapmak | [.NET 8 SDK (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) → **SDK** → **x64** |

İndirilen `.exe` dosyasını çalıştır ve kurulumu tamamla.

---

### 3. Veritabanı Erişimi

Uygulamanın bağlanabilmesi için:

- **MySQL** bağlantısı: sunucu adresi, port (varsayılan `3306`), kullanıcı adı ve şifre
- **MSSQL** bağlantısı: sunucu adresi, port (varsayılan `1433`), kullanıcı adı ve şifre

Bağlantı bilgilerini önceden hazır tutman yeterlidir; uygulama içinden elle girilebilir ya da `connections.json` dosyasına eklenebilir.

---

## Kurulum ve Çalıştırma

### Hazır exe ile çalıştırma (geliştirici değilseniz)

1. Projeyi [Releases](../../releases) sayfasından indirin.
2. ZIP'i bir klasöre çıkartın.
3. `DbQueryExplorer.exe` dosyasını çalıştırın.

> .NET 8 Desktop Runtime **yüklü değilse** uygulama açılmaz. Yukarıdaki kurulum adımlarını tamamlayın.

---

### Kaynak koddan derleme ve çalıştırma (geliştiriciler için)

**.NET 8 SDK** kurulu olmalıdır.

```bash
# Repoyu klonla
git clone <repo-url>
cd DbQueryExplorer

# Bağımlılıkları yükle ve çalıştır
dotnet run
```

Ya da Visual Studio 2022+ ile projeyi aç ve `F5` ile başlat.

---

## Bağlantı Profilleri

Sık kullanılan bağlantıları `connections.json` dosyasına ekleyebilirsin:

```json
[
  {
    "Name": "Yerel MySQL",
    "DbType": "MySQL",
    "Server": "localhost",
    "Port": 3306,
    "Database": "mydb",
    "Username": "root",
    "Password": "secret"
  },
  {
    "Name": "Prod MSSQL",
    "DbType": "MSSQL",
    "Server": "192.168.1.10",
    "Port": 1433,
    "Database": "ProductionDb",
    "Username": "sa",
    "Password": "secret"
  }
]
```

Uygulama başlarken bu dosyayı okur ve bağlantı listesine ekler.

---

## Kullanılan Teknolojiler

| Paket | Amaç |
|---|---|
| WPF (.NET 8) | Kullanıcı arayüzü |
| CommunityToolkit.Mvvm | MVVM altyapısı |
| MySqlConnector | MySQL bağlantısı |
| Microsoft.Data.SqlClient | MSSQL bağlantısı |
| ClosedXML | Excel (.xlsx) dışa aktarma |

---

## Lisans

İç kullanım amaçlı geliştirilmiştir.
