using BlogProjesi.Data;
using BlogProjesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogProjesi.Controllers
{
    [Authorize] // Sadece giriş yapanlar erişebilir
    public class MakaleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MakaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Makale/Detay/5
        [AllowAnonymous]
        public IActionResult Detay(int id)
        {
            var makale = _context.Makaleler
                .Include(m => m.Kullanici)
                .FirstOrDefault(m => m.MakaleId == id);

            if (makale == null) return NotFound();

            // Okunma sayısını artır
            makale.OkunmaSayisi++;
            _context.SaveChanges();

            return View(makale);
        }

        // POST: /Makale/Begen/5
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Begen(int id)
        {
            var makale = _context.Makaleler.Find(id);
            if (makale != null)
            {
                makale.BegeniSayisi++;
                _context.SaveChanges();
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: /Makale/Ekle
        public IActionResult Ekle()
        {
            return View();
        }

        // POST: /Makale/Ekle
        [HttpPost]
        public IActionResult Ekle(Makale model)
        {
            if (ModelState.IsValid)
            {
                // Giriş yapan kullanıcının ID'sini alıyoruz
                var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(kullaniciIdString, out int kullaniciId))
                {
                    model.KullaniciId = kullaniciId;
                    model.OlusturulmaTarihi = DateTime.Now;

                    _context.Makaleler.Add(model);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }

        // GET: /Makale/Duzenle/5
        public IActionResult Duzenle(int id)
        {
            var makale = _context.Makaleler.Find(id);
            if (makale == null) return NotFound();

            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = kullaniciIdString != null && int.Parse(kullaniciIdString) == makale.KullaniciId;

            if (!isOwner) return Forbid(); // Sadece yazar düzenleyebilir.

            return View(makale);
        }

        // POST: /Makale/Duzenle/5
        [HttpPost]
        public IActionResult Duzenle(int id, Makale model)
        {
            if (id != model.MakaleId) return NotFound();

            if (ModelState.IsValid)
            {
                var makale = _context.Makaleler.Find(id);
                if (makale == null) return NotFound();

                var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isOwner = kullaniciIdString != null && int.Parse(kullaniciIdString) == makale.KullaniciId;
                if (!isOwner) return Forbid();

                makale.Baslik = model.Baslik;
                makale.Icerik = model.Icerik;
                makale.ResimUrl = model.ResimUrl;

                _context.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // POST: /Makale/Sil/5
        [HttpPost]
        public IActionResult Sil(int id)
        {
            var makale = _context.Makaleler.FirstOrDefault(m => m.MakaleId == id);
            if (makale != null)
            {
                var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var rol = User.FindFirstValue(ClaimTypes.Role);

                var isOwner = kullaniciIdString != null && int.Parse(kullaniciIdString) == makale.KullaniciId;
                var canDeleteAny = rol == "Admin" || rol == "Yonetici";

                // Sadece kendi makalesini veya Admin/Yönetici ise silebilir
                if (isOwner || canDeleteAny)
                {
                    _context.Makaleler.Remove(makale);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
