using BlogProjesi.Data;
using BlogProjesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BlogProjesi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(string search, int? yazarId, string siralama = "yeni", int limit = 8)
        {
            // Filtreleme için tüm yazarları gönderelim (Dropdown için)
            ViewBag.Yazarlar = _context.Kullanicilar.ToList();
            
            // Mevcut filtre değerlerini View'a geri gönderelim (Inputlarda kalsın diye)
            ViewBag.Search = search;
            ViewBag.YazarId = yazarId;
            ViewBag.Siralama = siralama;
            ViewBag.Limit = limit;

            var sorgu = _context.Makaleler.Include(m => m.Kullanici).AsQueryable();

            // 1. Arama (Başlık veya İçerik)
            if (!string.IsNullOrEmpty(search))
            {
                sorgu = sorgu.Where(m => m.Baslik.Contains(search) || m.Icerik.Contains(search));
            }

            // 2. Yazar Filtreleme
            if (yazarId.HasValue)
            {
                sorgu = sorgu.Where(m => m.KullaniciId == yazarId.Value);
            }

            // 3. Sıralama
            if (siralama == "eski")
            {
                sorgu = sorgu.OrderBy(m => m.OlusturulmaTarihi);
            }
            else // varsayılan yeni
            {
                sorgu = sorgu.OrderByDescending(m => m.OlusturulmaTarihi);
            }

            // Toplam kayıt sayısını alalım (Daha fazla yükle butonu görünürlüğü için)
            ViewBag.ToplamMakale = sorgu.Count();

            // 4. Limit (Load More)
            var makaleler = sorgu.Take(limit).ToList();

            return View(makaleler);
        }

        public IActionResult DahaFazlaYukle(string search, int? yazarId, string siralama, int skip, int take = 8)
        {
            var sorgu = _context.Makaleler.Include(m => m.Kullanici).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                sorgu = sorgu.Where(m => m.Baslik.Contains(search) || m.Icerik.Contains(search));

            if (yazarId.HasValue)
                sorgu = sorgu.Where(m => m.KullaniciId == yazarId.Value);

            if (siralama == "eski")
                sorgu = sorgu.OrderBy(m => m.OlusturulmaTarihi);
            else
                sorgu = sorgu.OrderByDescending(m => m.OlusturulmaTarihi);

            var makaleler = sorgu.Skip(skip).Take(take).ToList();

            return PartialView("_MakaleListesi", makaleler);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
