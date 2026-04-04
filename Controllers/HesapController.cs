using BlogProjesi.Data;
using BlogProjesi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogProjesi.Controllers
{
    // Kullanıcı kayıt, giriş, çıkış ve profil güncelleme işlemlerini yöneten Controller.
    public class HesapController : Controller
    {
        // Entity Framework veritabanı bağlam nesnesi (Veritabanıyla köprü)
        private readonly ApplicationDbContext _context;

        // Dependency Injection (Bağımlılık Enjekte Etme Modeli) ile DbContext'i ASP.NET ayağa kalkarken otomatik alıyoruz.
        public HesapController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Hesap/Kayit
        // Kullanıcıya kayıt olacağı boş HTML formu sayfasını döndürür. Veritabanında işlem yapmaz.
        public IActionResult Kayit()
        {
            return View();
        }

        // POST: /Hesap/Kayit
        // Kullanıcı formu (Ad, Eposta, Şifre) doldurup "Kayıt Ol" butonuna bastığında (Submit) çalışır.
        // [HttpPost] action'ın form submit edildiğinde POST yöntemiyle çağrılacağını belirtir.
        [HttpPost]
        public IActionResult Kayit(Kullanici model)
        {
            // Gelen 'model' içindeki verilerin kurallara (Required, StringLength vb.) uygun olup olmadığını kontrol eder.
            if (ModelState.IsValid)
            {
                // Veritabanında aynı E-posta adresiyle kayıtlı başka biri var mı? (.Any() eşleşen bir sonuç bulursa true döner)
                if (_context.Kullanicilar.Any(x => x.Eposta == model.Eposta))
                {
                    // Hata mesajını sisteme ve Forma (ModelState'e) ekleyip formu girilen bilgiler silinmesin diye geri döndürüyoruz.
                    ModelState.AddModelError("", "Bu e-posta adresi zaten kullanılıyor.");
                    return View(model); // Form hatalı durumuyla geri açılır
                }

                // Aynı e-postadan yoksa, yeni kullanıcı verisini veritabanına eklenmek üzere hafızada kuyruğa al.
                _context.Kullanicilar.Add(model);
                
                // Kuyruktaki işlemi veritabanına zorunlu olarak uygula (Kayıt - Insert başarılı)
                _context.SaveChanges();
                
                // Başarılı kayıttan sonra kullanıcıyı doğrudan giriş yapacağı "Giris" sayfasına yönlendirir.
                return RedirectToAction("Giris");
            }
            // Eğer ModelState geçersizse (örn: eposta formatı bozuk), formu girilen verilerle birlikte hata gösterimi için View'a geri fırlat.
            return View(model);
        }

        // GET: /Hesap/Giris
        // Kullanıcının sisteme giriş yapacağı boş formu açan metot.
        public IActionResult Giris()
        {
            return View();
        }

        // POST: /Hesap/Giris
        // Kullanıcı e-posta ve şifresini yazıp 'Giriş' formunu gönderdiğinde çalışır. 
        // Asenkron ('async Task', 'await') çalışır çünkü oturum açma işlemleri sistemden şifreleme ve cookie üretmek için bağımsız zaman ister.
        [HttpPost]
        public async Task<IActionResult> Giris(string eposta, string sifre)
        {
            // Kullanıcının Eposta'sı ve Sifre'si veritabanındaki ile eşleşiyor mu kontrol et. 
            // (FirstOrDefault eşleşen ilk kaydı getirir, hiçbir eşleşme yoksa 'null' döndürür).
            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Eposta == eposta && x.Sifre == sifre);

            // Eğer eşleşme varsa (kullanıcı null gelmediyse; Eposta ve şifre doğru demektir)
            if (kullanici != null)
            {
                // Kimlik bilgilerini taşıyan ve hafızada şifreli olarak tutulacak olan "Claim" (Hak/Beyan) listesini oluşturuyoruz.
                // Sistemi performanslı hale getirmek için, her sayfada bu kullanıcı Admin mi diye veritabanından çekmek yerine, yetkisi ve kimliği bir 'Cookie' içine gömülerek bilgisayarla dolaşır.
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString()), // ID bilgisini saklar (yazar kime ait vs. ilişkiler için)
                    new Claim(ClaimTypes.Name, kullanici.AdSoyad),                           // Ad-Soyad (Üst menüde/navbar'da kişinin İsmini göstermek için)
                    new Claim(ClaimTypes.Email, kullanici.Eposta),                           // E-posta bilgisi
                    new Claim(ClaimTypes.Role, kullanici.Rol)                                // En önemlisi - Yetki (Rol) bilgisi (Örn: Yetkisiz birisi Admin paneline giremesin diye)
                };

                // Claim'leri ASP.NET Core'un varsayılan Çerez (Cookie) doğrulama mimarisine yüklüyoruz.
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // HttpContext.SignInAsync ile oturumu aç (Burası şifreli Güvenlik Kimliği/Çerezi (Cookie) üretilip o anki tarayıcıya bırakıldığı satırdır)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Oturum başarıyla açıldı, anasayfaya (Home Controller'ın Index ekranına) sevk et.
                return RedirectToAction("Index", "Home");
            }

            // Eğer dönen 'kullanici' null ise Eposta veya Şifre veritabanıyla uyuşmuyor demektir, hata mesajı göster ve boş forma geri yolla.
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View();
        }


        // GET: /Hesap/Profil
        // Giriş yapmış kullanıcının kendi profil (Güncelleme) bilgilerini gördüğü sayfa.
        // '[Authorize]' eklenmiştir, çünkü yalnızca sisteme giriş yapmış (geçerli cookie'si olan) kişiler profilini görebilir (Giriş yapmayanları login'e atar).
        [Authorize]
        public IActionResult Profil()
        {
            // Şu anda sisteme giriş yapmış olan kullanıcının ID'sini veritabanına gitmeden, kendi tarayıcı çerezinden (ClaimTypes.NameIdentifier) okuyarak alıyoruz.
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            
            // Profilindeki güncel bilgileri (şifresi, rolleri vs) alıp form metin kutularına doldurmak için veritabanından .Find ile kişi satırını çekiyoruz.
            var kullanici = _context.Kullanicilar.Find(userId);
            if (kullanici == null) return NotFound(); // Kayıt bulamadıysa Hata fırlat (Nadiren olur, üye bu oturumdayken admin onu arkadan silmişse vb.)

            // Bulunan kullanıcı verisini sayfaya (Güncelleme Formuna) gönder
            return View(kullanici);
        }

        // POST: /Hesap/Profil
        // Profil sayfasında değiştirilip "Güncelle" dendiğinde (Post) çalışacak metot.
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profil(Kullanici model)
        {
            // Güncellemeyi yapan kişinin kendi bilgileri olduğundan emin olmak için "bizzat giriş yapmış kullanıcının" ID'sini güvenli çerezden tekrar alıyoruz. (Hacklemeye karşı başkasının ID'si gelmesin diye)
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            
            // Kullanıcıyı veritabanından referans olarak bul (Hafızaya çek, çünkü bu kayıt Update edilecek).
            var kullanici = _context.Kullanicilar.Find(userId);

            if (kullanici != null)
            {
                // Gelen form verilerini (model) veritabanı yansıması (kullanici) üzerine eşitle.
                kullanici.AdSoyad = model.AdSoyad;
                kullanici.Eposta = model.Eposta;
                
                // Eğer şifre alanı formdan boş gönderilmediyse (Yani kullanıcı yeni şifre de girmişse) şifreyi de değiştir/ata.
                if (!string.IsNullOrEmpty(model.Sifre))
                {
                    kullanici.Sifre = model.Sifre;
                }

                // Değişiklikleri veritabanına Update sorgusu atarak "kalıcı" olarak yansıt.
                _context.SaveChanges();

                // ÇEREZİN (OTURUMUN) YENİLENMESİ:
                // Kullanıcı adını veya epostasını değiştirdiği için, üst menüde (Navbar) hemen güncellenen isminin gözükmesi gerekir. Ancak eski isim Cookie'de kalmıştı.
                // Bu yüzden yeni bilgilerle aynı Claim listesini oluşturuyoruz ki yeni bir Cookie üretelim.
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, kullanici.AdSoyad),
                    new Claim(ClaimTypes.Email, kullanici.Eposta),
                    new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString()),
                    new Claim(ClaimTypes.Role, kullanici.Rol) // Rolü değişirse anında yetkileri yenilensin diye
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Kalıcı oturum özellikleri
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                // Önceki (Eski bilgili) kimlik çerezini sistemden/tarayıcıdan at (SignOut)
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Güncellenen Yeni kimlik çerezini aynı isme sahip olarak tarayıcıya tekrar yükle (Oturumu tazele)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // 'TempData', yönlendirme yapılan (Redirect) yeni sayfada tek kullanımlık bir 'Bilgi Mesajı' oluşturmak için kullanılır ve ekranda ilk göründükten (okunduktan) sonra kendi kendini siler.
                TempData["Mesaj"] = "Bilgileriniz başarıyla güncellendi.";

                return RedirectToAction("Index", "Home");
            }

            // Hata çıkarsa bilgilerin bulunduğu forma (Profil) geri dön
            return View(model);
        }

        // GET/POST: /Hesap/Cikis
        // Sistemden Çıkış (Logout/Yetkileri Sıfırlama) işlemi
        public async Task<IActionResult> Cikis()
        {
            // HttpContext.SignOutAsync, tarayıcıda bulunan Kimlik (Authentication) çerezini ve Auth oturumunu imha eder.
            // Bu sayede sistem kullanıcının giriş yapmadığını (Misafir) olduğunu algılamaya başlar.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Çıkış yaptıktan sonra anasayfaya yönlendir.
            return RedirectToAction("Index", "Home");
        }

    }
}
