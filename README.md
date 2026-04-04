Here's the improved `README.md` file, incorporating the new content while maintaining the existing structure and coherence:

# BlogProjesi

Basit bir blog uygulaması. Bu README projede kullanılan teknolojileri, proje yapısını, önemli sayfaları, veri modellerini, yerel kurulum ve çalıştırma adımlarını içerir.

## Kullanılan Teknolojiler

- .NET 8 (ASP.NET Core)
- ASP.NET Core MVC ile Razor Görünümleri
- Entity Framework Core (EF Core) + SQL Server
- Cookie Authentication (Çerez tabanlı oturum)
- Bootstrap 5 ve Bootstrap Icons
- jQuery (site scriptleri için)

## Proje Özeti

`BlogProjesi`, öğrenme amaçlı geliştirilmiş bir blog platformudur. Kullanıcı hesapları, makale (makale) listesi, arama, yazar ile filtreleme, "daha fazla yükle" şeklinde yükleme davranışı ve yönetici (Admin) yetkileri içerir.

Öne çıkan özellikler:
- Razor görünümleri ve kısmi görünümlerle sunucu tarafı render
- Ana sayfada başlık/icerik arama, yazar filtresi ve sıralama
- "Daha Fazla Yükle" ile istemci tarafı yükleme (fetch)
- Çerez tabanlı kimlik doğrulama
- Geliştirme kolaylığı için uygulama başlangıcında veritabanı otomatik oluşturma

## Önemli Dosyalar ve Sayfalar

- `Program.cs` — Uygulama başlangıcı, servis ve middleware konfigürasyonu. Varsayılan admin kullanıcısı tohumlanır (`admin@example.com`).
- `Data/ApplicationDbContext.cs` — EF Core `DbContext`, `Kullanicilar` ve `Makaleler` DbSet'lerini sağlar.

Görünümler ve bileşenler:
- `Views/Home/Index.cshtml` — Ana sayfa ("Ana Sayfa"): arama, sıralama, yazar filtresi ve makale ızgarası. "Daha Fazla Yükle" davranışı içerir.
- `Views/Home/_MakaleListesi.cshtml` — Makale listesini döngü ile render eden kısmi görünüm.
- `Views/Shared/_Layout.cshtml` — Genel layout: navigasyon, oturum linkleri, admin linki, footer ve statik varlıklar.
- `Views/Shared/_MakaleKart.cshtml` — Tek bir makale kartının kısmi görünümü (liste tarafından kullanılır).

Kontroller (projede referans verilen):
- `HomeController` — `Index`, `DahaFazlaYukle`, `Privacy` vb.
- `HesapController` — `Giris`, `Kayit`, `Cikis`, `Profil` gibi kimlik işlemleri.
- `MakaleController` — Makale oluşturma/duzenleme (`Ekle`, vb.).
- `AdminController` — Yönetici işlemleri (`Kullanicilar`) — yalnızca `Admin` rolü görünür.

## Veri Modelleri (özeti)

- `Kullanici`
  - `KullaniciId` (int) — PK
  - `AdSoyad` (string, zorunlu)
  - `Eposta` (string, zorunlu, Email)
  - `Sifre` (string, zorunlu)
  - `Rol` (string, varsayılan: "Yazar")
  - `Makaleler` (ICollection<Makale>) — navigation

- `Makale`
  - `MakaleId` (int) — PK
  - `Baslik` (string, zorunlu)
  - `Icerik` (string, zorunlu)
  - `ResimUrl` (string?)
  - `OkunmaSayisi` (int)
  - `BegeniSayisi` (int)
  - `OlusturulmaTarihi` (DateTime)
  - `KullaniciId` (int) — FK
  - `Kullanici` (Kullanici?) — navigation

## Yerel Kurulum

1. Gereksinimler
   - .NET 8 SDK
   - SQL Server veya LocalDB
   - Visual Studio 2022 veya VS Code

2. Bağlantı dizesi
   - `appsettings.json` içinde `DefaultConnection` anahtarını kendi SQL Server bağlantınıza göre güncelleyin.

3. Çalıştırma
   - Visual Studio: Çözümü açın ve F5 ile çalıştırın veya Ctrl+F5 ile başlatın.
   - CLI: Proje klasöründe `dotnet run` komutunu çalıştırın.

Not: Uygulama başlangıcında `context.Database.EnsureCreated()` çalışarak veritabanını otomatik oluşturur. Üretime geçmeden önce EF Migrations kullanılmasını ve otomatik tohumlanan şifrelerin güvenli şekilde değiştirilmesini öneririm.

## Kimlik Doğrulama ve Tohumlanan Admin

- Çerez tabanlı kimlik doğrulama `Program.cs` içinde yapılandırılmıştır (`CookieAuthenticationDefaults.AuthenticationScheme`).
- Varsayılan admin kullanıcı: `admin@example.com` / `admin`. Yerel test amaçlı oluşturulur; üretime almadan önce değiştirin.

## Katkı

Kodlama standartlarına uyunuz. `CONTRIBUTING.md` veya `.editorconfig` eklenmesini isterseniz talep açın veya PR gönderin.

## Lisans

Projeyi yayımlamadan önce uygun bir lisans dosyası ekleyin.
