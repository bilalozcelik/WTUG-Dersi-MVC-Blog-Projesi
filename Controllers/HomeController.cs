using BlogProjesi.Data;
using BlogProjesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BlogProjesi.Controllers
{
    // Projenin giriş noktası. Ziyaretçilerin (misafirlerin) göreceği anasayfayı ve liste akışını yönetir.
    public class HomeController : Controller
    {
        // Geliştirme süresinde konsola, log dosyalarına uyarı veya bilgi basabilmemizi sağlayan Microsoft aracı.
        private readonly ILogger<HomeController> _logger;
        
        // Entity Framework veritabanı bağlam nesnesi. Veritabanına Select, Insert yollayabilmek için gereklidir.
        private readonly ApplicationDbContext _context;

        // Dependency Injection ile Logger ve DbContext sınıfları nesne haline getirilip yapıcı (constructor) metottan alınır.
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /Home/Index
        // Anasayfanın yüklendiği ve arama/filtreleme işlemlerinin (URL parametreleriyle) yakalandığı ana metot.
        // 'search': Formdan gelen arama kelimesi, 'yazarId': Dropdowndan seçilen Yazar, 'siralama': yeni/eski radyo butonları, 'limit': İlk etapta kaç tane yükleneceği.
        public IActionResult Index(string search, int? yazarId, string siralama = "yeni", int limit = 8)
        {
            // Dropdown (menü) listesi için sadece 'Görünüm' (View) tarafında kullanılacak tüm yazarları (Kullanicilari) çekip ViewBag ile sayfaya serbestçe (Modelsiz) fırlatıyoruz.
            ViewBag.Yazarlar = _context.Kullanicilar.ToList();
            
            // Eğer arama yapılmışsa formlar (inputlar) boşalmasın diye gönderdikleri filtre bilgisini yine onlara ViewBag ile iade ediyoruz ki ekranda kalsın.
            ViewBag.Search = search;
            ViewBag.YazarId = yazarId;
            ViewBag.Siralama = siralama;
            ViewBag.Limit = limit;

            // ".Include(m => m.Kullanici)": Makale tablosuna giderken yazarının verilerini de (Inner Join yaparak) veritabanından çek. 
            // "AsQueryable()": Sorguyu henüz bitirme, aşağıda If bloklarıyla yeni Where kuralları ekleyeceğiz demek.
            var sorgu = _context.Makaleler.Include(m => m.Kullanici).AsQueryable();

            // 1. Arama Şartı (Eğer arama kutusu doluysa veritabanına LIKE %% şartı ekler)
            if (!string.IsNullOrEmpty(search))
            {
                // 'm' bir makaleyi temsil eder. Başlığında veya İçeriğinde bu kelime geçiyor mu? (OR || mantığı)
                sorgu = sorgu.Where(m => m.Baslik.Contains(search) || m.Icerik.Contains(search));
            }

            // 2. Yazar Filtresi (Birisi seçilmiş ise)
            if (yazarId.HasValue) // (int? nullable olduğu için HasValue ile doluluğu denenir)
            {
                // Sadece veritabanındaki KullaniciId'si bizim seçtiğimiz 'yazarId'ye eşleşenleri getir. 
                sorgu = sorgu.Where(m => m.KullaniciId == yazarId.Value);
            }

            // 3. Sıralama Şartı (ORDER BY sql komutunun eşdeğeridir)
            if (siralama == "eski")
            {
                sorgu = sorgu.OrderBy(m => m.OlusturulmaTarihi); // Eskiden yeniye (ASC)
            }
            else // Varsayılan olan "yeni" veya başka sapan bir kelime gelirse
            {
                sorgu = sorgu.OrderByDescending(m => m.OlusturulmaTarihi); // Yeniden eskiye (DESC)
            }

            // Sorgunun sonunda toplam kaç makale kalacağını hesapla (Count()).
            // Bu rakamı, anasayfada "Daha Fazla Yükle" butonunu ne zaman gizleyeceğimizi göstermek için kullanacağız.
            ViewBag.ToplamMakale = sorgu.Count();

            // 4. Limitli Çekim (Bütün hepsi süzüldükten sonra sonuçtan ilk listesi (sadece 'limit' kadarı - örn: 8 tane) ToList() ile tam o anda veritabanından alınır.
            var makaleler = sorgu.Take(limit).ToList();

            // Süzülen Listeyi Anysayfa görünümüne (Index.cshtml) gönder.
            return View(makaleler);
        }

        // GET: /Home/DahaFazlaYukle (Genelde AJAX ile tetiklenir)
        // Sayfayı yenilemeden arkadan veri yüklemek (Load More) için kullanılır.
        public IActionResult DahaFazlaYukle(string search, int? yazarId, string siralama, int skip, int take = 8)
        {
            // Yukarıdaki index sorgularını aynen inşa et (Tekrar filtreden geçir ki aranan şeye göre sonrakiler gelsin)
            var sorgu = _context.Makaleler.Include(m => m.Kullanici).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                sorgu = sorgu.Where(m => m.Baslik.Contains(search) || m.Icerik.Contains(search));

            if (yazarId.HasValue)
                sorgu = sorgu.Where(m => m.KullaniciId == yazarId.Value);

            if (siralama == "eski")
                sorgu = sorgu.OrderBy(m => m.OlusturulmaTarihi);
            else
                sorgu = sorgu.OrderByDescending(m => m.OlusturulmaTarihi);

            // ÖNEMLİ KISIM (Skip ve Take): 
            // Daha önce ekranda kaç tane olduğunu bildiğimiz için (Skip), oraya kadar olan makaleleri ATLA ve ondan sonraki 'take' adedini al.
            var makaleler = sorgu.Skip(skip).Take(take).ToList();

            // Dikkat: Bu metot tüm anasayfayı değil, sadece Partial View (Parçalı Görünüm) dediğimiz "Kart" HTML parçasını döndürür. (Sadece o parçalar Ajax ile dom'a eklenir)
            return PartialView("_MakaleListesi", makaleler);
        }

        // Gizlilik politikası metninin olduğu görünümü döner. (Statik sayfa)
        public IActionResult Privacy()
        {
            return View();
        }

        // Proje ayağa kalktığında Global bir hata patlarsa ASP.NET burayı çağırır. (Catch bloğu gibi düşünülebilir)
        // Cachelenmemesini (tarayıcı önbelleğine atılmamasını) söyler ki canlı hata görülsün.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
