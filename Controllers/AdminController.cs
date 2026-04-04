using BlogProjesi.Data;
using BlogProjesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogProjesi.Controllers
{
    // [Authorize] niteliği, bu sayfadaki işlemlere (Action'lara) yalnızca giriş yapmış 
    // ve rolü (Roles) "Admin" olan kişilerin erişebileceğini belirtir.
    // Başka bir role sahip kullanıcı veya ziyaretçi erişmek isterse "Erişim Reddedildi (Access Denied)" hatası alır.
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // Entity Framework Core'un veritabanı işlemlerini yürüttüğü bağlantı bağlamı (Context) nesnesi.
        private readonly ApplicationDbContext _context;

        // Dependency Injection (Bağımlılık Enjeksiyonu): 
        // Controller oluşturulduğunda sistem otomatik olarak ApplicationDbContext nesnesini (veritabanını) constructor (yapıcı metot) aracılığıyla buraya gönderir.
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Kullanicilar
        // Amacı: Veritabanındaki tüm kullanıcıları listelemek ve bunları yönetim sayfasına (View) göndermek.
        public IActionResult Kullanicilar()
        {
            // Veritabanındaki Kullanicilar tablosundaki tüm kayıtları RAM'e liste (.ToList()) olarak çeker.
            var kullanicilar = _context.Kullanicilar.ToList();
            
            // Çekilen bu listeyi (Modeli), ekranda gösterilmek üzere View'a (Görünüm'e) gönderir.
            return View(kullanicilar);
        }

        // POST: /Admin/KullaniciSil/5
        // Amacı: Form üzerinden (gizli olarak) gelen id bilgisini alıp, veritabanından bulup o kullanıcıyı silmektir.
        // [HttpPost] güvenliği artırarak metodun sadece POST istekleriyle (form gönderimi) çalışmasını sağlar, link üzerinden silme yapılamaz.
        [HttpPost]
        public IActionResult KullaniciSil(int id)
        {
            // Gelen id'ye göre kullanıcıyı veritabanında (Primary Key ile) bul. RAM'e Getir (.Find).
            var kullanici = _context.Kullanicilar.Find(id);
            if (kullanici != null) // Eğer o id'de bir kullanıcı gerçekten varsa
            {
                // Admin'in kendini yanlışlıkla silmesini engellemek için güvenlik kontrolü (Mantıksal Güvenlik)
                if (kullanici.Rol != "Admin")
                {
                    // Entity Framework Remove komutu ile kaydı silinecek olarak işaretle. (Hafızada işlem)
                    _context.Kullanicilar.Remove(kullanici);
                    
                    // İşaretlenen değişikliği asıl veritabanına uygula (Fiziksel SQL Delete komutu bu satırda çalışır)
                    _context.SaveChanges();
                }
            }
            // İşlem bittikten sonra tekrar Kullanicilar liste sayfasına (GET metoduna) yeniden yönlendir.
            return RedirectToAction("Kullanicilar");
        }

        // POST: /Admin/RolDegistir/5
        // Amacı: Form üzerinden gelen id ve 'yeniRol' (string) seçimine göre kullanıcının rolünü (Yetkisini) güncellemek.
        [HttpPost]
        public IActionResult RolDegistir(int id, string yeniRol)
        {
            // Kullanıcıyı veritabanından bul (SELECT işlemi ile)
            var kullanici = _context.Kullanicilar.Find(id);
            
            if (kullanici != null)
            {
                // Başka bir adminin (veya kendisinin) rolünü düşürmesini engellemek için isteğe bağlı güvenlik kontrolü
                if (kullanici.Rol != "Admin") 
                {
                    // Seçilen yeni rolü kullanıcının Rol alanına atıyoruz (Hafızadaki nesne güncellenir)
                    kullanici.Rol = yeniRol;
                    
                    // Değişiklikleri Veritabanına kaydet/güncelle (SQL Update komutu bu satırda çalışır)
                    _context.SaveChanges();
                }
            }
            // Güncelleme bitince yine listeye dönerek yenilenen veriyi göster.
            return RedirectToAction("Kullanicilar");
        }
    }
}
