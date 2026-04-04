using BlogProjesi.Data;
using BlogProjesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogProjesi.Controllers
{
    // Sınıf seviyesindeki [Authorize], bu Controller altındaki tüm "Makale/XXX" yollarına 
    // sadece sisteme "Giriş" yapmış kişilerin girebileceği kuralını genel bir şemsiye gibi koyar.
    [Authorize] 
    public class MakaleController : Controller
    {
        // Entity Framework Core'un veritabanına bağlandığı nesne
        private readonly ApplicationDbContext _context;

        // Dependency Injection üzerinden veritabanı alınıyor.
        public MakaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Makale/Detay/5
        // [AllowAnonymous] sınıf seviyesindeki [Authorize] kuralını burası için ESNETİR.
        // Yani üye olmayan ziyaretçiler (misafirler) de makale "Detay"ını görebilmeli.
        [AllowAnonymous]
        public IActionResult Detay(int id)
        {
            // URL'deki "id" bilgisine sahip makaleyi, kullanıcısına da(yazarına) Join ederek (Include) veritabanından ara (Aksi halde null döner).
            var makale = _context.Makaleler
                .Include(m => m.Kullanici)
                .FirstOrDefault(m => m.MakaleId == id);

            // Eğer id yalanlandıysa veya makale silindiyse 404 Sayfa Bulunamadı sayfasına at.
            if (makale == null) return NotFound();

            // Makalenin detay sayfasına girildiği için, veritabanına yansıyacak olan "Okuma Sayısını" bir artır.
            makale.OkunmaSayisi++;
            
            // Değişikliği (Update'i) kaydet.
            _context.SaveChanges();

            // Makaleyi detay sayfasında gösterilmek üzere View'a ilet.
            return View(makale);
        }

        // POST: /Makale/Begen/5
        // Kullanıcı Kalp/Beğen butonuna tıklandığında görünmeyen bir POST form ile çalışır.
        [HttpPost]
        [AllowAnonymous] // Beğenmek için illa giriş yapmaya gerek yok seçeneği (Müşteri tercihidir, isteğe göre kaldırılabilir)
        public IActionResult Begen(int id)
        {
            var makale = _context.Makaleler.Find(id); // Makaleyi bul
            if (makale != null)
            {
                // Beğeni parametresini artır
                makale.BegeniSayisi++;
                
                // Ve veritabanına doğrudan kaydet
                _context.SaveChanges();
            }
            
            // Beğendikten sonra, hangi sayfadan basmış olursa olsun Index (Anasayfa)'ya fırlat (veya detay sayfasına yönlendirme de verilebilirdi)
            return RedirectToAction("Index", "Home");
        }

        // GET: /Makale/Ekle
        // Makale oluşturma sayfasını (Formunu) ekranda görüntülemek içindir.
        // ([Authorize] zaten bu metodu yukarıdan korumaya almıştır, misafir giremez).
        public IActionResult Ekle()
        {
            return View();
        }

        // POST: /Makale/Ekle
        // Form 'Gönder'lendiğinde (Submit) çalışır. Gelen yazi verileri 'model' içerisindedir.
        [HttpPost]
        public IActionResult Ekle(Makale model)
        {
            // Form kuralları (Required, başlık uzunluğu vb.) doğru ise kaydetmeye başla
            if (ModelState.IsValid)
            {
                // ÖNEMLİ PÜF NOKTASI: 
                // Biz formu eklerken "KullaniciId" kutusu koymadık çünkü kişinin kendi id'si tarayıcı çerezinde(Cookie'deyken) var. Başkası gizlice araya girmesin diye bizzat oturum çerezinden sistem alıyor:
                var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Cookie'deki ID'yi oku.
                
                // ID bir rakam mı? kontrol et ve alabiliyorsan integer'a çevir (TryParse=güvenli çevirim)
                if (int.TryParse(kullaniciIdString, out int kullaniciId))
                {
                    // Makalenin asıl yazarının ID'sini model'e (Veritabanına eklenecek nesneye) yapıştır (Aksi halde SQL hata verirdi).
                    model.KullaniciId = kullaniciId;
                    
                    // Eklenme anını o anki sunucu saatine eşitle
                    model.OlusturulmaTarihi = DateTime.Now;

                    // Yeni makaleyi bellekte sıraya al
                    _context.Makaleler.Add(model);
                    
                    // İşlemi Veritabanına (Insert komutuyla) kalıcı yansıt
                    _context.SaveChanges();
                    
                    // Kaydettikten sonra makalelerin listelendiği Ana Sayfaya yönlendir
                    return RedirectToAction("Index", "Home");
                }
            }
            // Herhangi bir kural (Örn: Model eksikliği) engeline takılırsa, girdiği verilerle beraber aynı 'Ekle' form sayfasına geri gönder (Sil baştan başlamasın).
            return View(model);
        }

        // GET: /Makale/Duzenle/5
        // Düzenlenecek Makalenin eski bilgilerini bulup formun içine doldurtmak içindir.
        public IActionResult Duzenle(int id)
        {
            // Makaleyi bul 
            var makale = _context.Makaleler.Find(id);
            if (makale == null) return NotFound();

            // YETKİ KONTROLÜ (Authorisation)
            // Düzenleme sayfasına adres satırından (örn /Makale/Duzenle/7) giren kişinin gerçeten bu makalenin YAZARI olup olmadığı kontrol edilir. Başkasına ait numarayı tuşlamış olabilir mi?
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = kullaniciIdString != null && int.Parse(kullaniciIdString) == makale.KullaniciId; // O an girdiği cookie'deki id = makalenin veritabanındaki KullaniciId siyle aynı mı?

            if (!isOwner) return Forbid(); // Eğer aynı değilse (sahtekarlık), Forbid (403 Yasak / Yetkisiz Ziyaret) sayfası dön. "Sadece kendi makaleni düzeltebilirsin"

            // Ekranda düzenlenmesi için Makaleyi modele (form sayfasına) ilet.
            return View(makale);
        }

        // POST: /Makale/Duzenle/5
        // Form 'Güncelle' butonuyla post edildiğinde çalışır ve veriyi asıl değiştiren satırdır.
        [HttpPost]
        public IActionResult Duzenle(int id, Makale model)
        {
            // URL'deki ID ile Gizli formdan (Input hidden) gelen ID aynı değilse reddet (Hack girişimi vb).
            if (id != model.MakaleId) return NotFound();

            // Kurallara uygun form yollandıysa
            if (ModelState.IsValid)
            {
                var makale = _context.Makaleler.Find(id); // Hafızaya eski olanı çek (Update aşaması 1)
                if (makale == null) return NotFound();

                // YETKİ KONTROLÜ - Birisi GET yöntemini ezemedi diye POST yöntemine sunucu altından veri şutlarsa, POST kısmında da sadece "Sahibi" olan düzeltsin diye bir güvenlik sorugusu daha yapılır. (Solid ve Siber güvenlik mimarisi gereği)
                var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isOwner = kullaniciIdString != null && int.Parse(kullaniciIdString) == makale.KullaniciId;
                if (!isOwner) return Forbid();

                // Gelen yeni form verilerini, eski veritabanı yansımasının üzerine kopyalıyoruz (Eşitleme/Senkron)
                makale.Baslik = model.Baslik;
                makale.Icerik = model.Icerik;
                makale.ResimUrl = model.ResimUrl;

                // Entity Framework Core sayesinde sadece güncellenen hücreler için SQL Update kodu arka planda otomatik yazılıp kaydedilir.
                _context.SaveChanges();
                
                // Başarılı ise Ana sayfaya fırlat
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // POST: /Makale/Sil/5
        // Bir makale silme isteğinde bulunulduğunda (Burası GET olmadığı için adres satırına link yazarak silinemez, Butona POST edilmesi gerekir. (Güvenlik))
        [HttpPost]
        public IActionResult Sil(int id)
        {
            // Silinecek olanı bul
            var makale = _context.Makaleler.FirstOrDefault(m => m.MakaleId == id);
            if (makale != null)
            {
                // Cookie'den kişinin ID'sini ve ROLÜNÜ oku. (Admin bileşenleri vb. için)
                var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var rol = User.FindFirstValue(ClaimTypes.Role);

                // Bu kişi(Cookie sahibİ) makalenin BİZZAT YAZARI mı?
                var isOwner = kullaniciIdString != null && int.Parse(kullaniciIdString) == makale.KullaniciId;
                
                // VEYA bu kişi bir Admin/Yonetici mi (Başkasının makalesini silebilme yetkisi)?
                var canDeleteAny = rol == "Admin" || rol == "Yonetici";

                // Eğer kişi kendi makalesini SİLİYORSA veya Admin olarak uygunsuz bulduğu birinin makalesini siliyorsa (OR || bağlacı) işleme izin ver:
                if (isOwner || canDeleteAny)
                {
                    // Makaleyi Entity Framework listelerinden silinecekler kuyruğuna koy (Delete SQL komutuna hazırlanır)
                    _context.Makaleler.Remove(makale);
                    
                    // Kalıcı olarak fiziksel boyutta SQL tarafında silinir.
                    _context.SaveChanges();
                }
            }
            // Kayıt listesinin veya anasayfanın tazelenmesi için Home Controller'a yönlendir.
            return RedirectToAction("Index", "Home");
        }
    }
}
