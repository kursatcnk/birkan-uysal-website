# Birkan Uysal | Kişisel Web Sitesi

Makine mühendisi Birkan Uysal için kişisel web sitesi ve sitenin her bölümünü düzenleyen yönetim paneli. ASP.NET Core (.NET 10) ile yazıldı; veritabanı gerektirmez, içerik tek bir JSON dosyasında tutulur.

## Yapı

```
Birkan.Web/
  App_Data/birkan-content.json   Sitedeki bütün metinler ve görsel yolları
  Models/BirkanContent.cs        İçerik modeli
  Services/                      İçerik deposu, panel hesabı, görsel işleme, güvenli yol/bağlantı kontrolleri
  Controllers/                   Site
  Panel/                         Yönetim paneli (denetleyici, form alanları, Atatürk içerikleri)
  Views/BirkanSite/              Site şablonu
  Views/BirkanPanel/             Panel sayfaları
  wwwroot/css, js                Site stilleri ve davranışları
  wwwroot/birkan-panel/          Panel stilleri, davranışları ve görselleri
  wwwroot/birkan-media/          Fotoğraflar, yetenek ikonları, yüklenen görseller
```

## Çalıştırma

İlk açılışta panel hesabı ayarlardan oluşturulur. Kullanıcı adı ve şifre repoda tutulmaz; yerelde user-secrets ile verilir:

```bash
cd Birkan.Web
dotnet user-secrets set "BirkanPanel:Username" "<kullanıcı adı>"
dotnet user-secrets set "BirkanPanel:InitialPassword" "<şifre>"
dotnet run
```

Site `http://localhost:5169`, panel `http://localhost:5169/birkan-panel` adresinde açılır.

Sunucuda aynı ayarlar ortam değişkeniyle verilir: `BirkanPanel__Username` ve `BirkanPanel__InitialPassword`. Hesap oluştuktan sonra şifre panelden değiştirilebilir; hash `App_Data/birkan-admin.json` dosyasında saklanır.

## Panel

- **Karargâh:** Özet, günün Atatürk sözü, Atatürk kronolojisi ve hızlı düzenleme kısayolları
- **Genel ve Giriş:** İsim, unvan, sekme başlığı, logo, giriş fotoğrafı, sosyal bağlantılar, menü adları
- **Hakkımda, Eğitim, Deneyim, Yetenekler, Projeler, İletişim:** Her bölümün metinleri; öğe ekleme, silme ve sıralama; okul ve şirket logoları
- **Yedekler:** Her kayıttan önce içeriğin kopyası alınır, son 30 kopya geri yüklenebilir
- **Şifre:** Panel şifresini değiştirme

Yüklenen görseller yeniden çizilip WebP olarak kaydedilir; konum gibi meta veriler silinir. Giriş denemeleri IP başına sınırlıdır, 5 hatalı denemede hesap 15 dakika kilitlenir.

## Yayına alma

Sunucuda `App_Data/` ve `wwwroot/birkan-media/uploads/` klasörlerinin uygulama tarafından yazılabilir olması gerekir. Panelden yapılan değişiklikler bu klasörlerde durduğu için yeni bir sürüm yüklerken bu iki klasörün üzerine yazılmamalıdır.

## Görseller

Paneldeki Atatürk fotoğrafları ve imzası Wikimedia Commons'tan alınmış kamu malı görsellerdir.

## Not

Sitenin ilk sürümü Nisan 2026'da geliştirildi ve o dönem GitHub'a yüklenmemişti. Nisan commit'leri eldeki son yerel kopyadan oluşturuldu; ara tarihler yaklaşıktır.
