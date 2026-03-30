using BlogProjesi.Data;
using BlogProjesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogProjesi.Controllers
{
    // Sadece rolü Admin olanlar bu kontrolcüye ulaşabilir
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Kullanicilar
        public IActionResult Kullanicilar()
        {
            var kullanicilar = _context.Kullanicilar.ToList();
            return View(kullanicilar);
        }

        // POST: /Admin/KullaniciSil/5
        [HttpPost]
        public IActionResult KullaniciSil(int id)
        {
            var kullanici = _context.Kullanicilar.Find(id);
            if (kullanici != null)
            {
                // Admin kendini silemesin mantıklı olabilir.
                if (kullanici.Rol != "Admin")
                {
                    _context.Kullanicilar.Remove(kullanici);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Kullanicilar");
        }

        // POST: /Admin/RolDegistir/5
        [HttpPost]
        public IActionResult RolDegistir(int id, string yeniRol)
        {
            var kullanici = _context.Kullanicilar.Find(id);
            if (kullanici != null)
            {
                if (kullanici.Rol != "Admin") // Başka bir adminin rolünü düşüremesin (isteğe bağlı güvenlik)
                {
                    kullanici.Rol = yeniRol;
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Kullanicilar");
        }
    }
}
