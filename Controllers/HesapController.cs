using BlogProjesi.Data;
using BlogProjesi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogProjesi.Controllers
{
    public class HesapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HesapController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Hesap/Kayit
        public IActionResult Kayit()
        {
            return View();
        }

        // POST: /Hesap/Kayit
        [HttpPost]
        public IActionResult Kayit(Kullanici model)
        {
            if (ModelState.IsValid)
            {
                // Aynı e-postadan var mı kontrolü
                if (_context.Kullanicilar.Any(x => x.Eposta == model.Eposta))
                {
                    ModelState.AddModelError("", "Bu e-posta adresi zaten kullanılıyor.");
                    return View(model);
                }

                _context.Kullanicilar.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Giris");
            }
            return View(model);
        }

        // GET: /Hesap/Giris
        public IActionResult Giris()
        {
            return View();
        }

        // POST: /Hesap/Giris
        [HttpPost]
        public async Task<IActionResult> Giris(string eposta, string sifre)
        {
            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Eposta == eposta && x.Sifre == sifre);

            if (kullanici != null)
            {
                // Kimlik bilgilerini (Claims) oluşturuyoruz.
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString()),
                    new Claim(ClaimTypes.Name, kullanici.AdSoyad),
                    new Claim(ClaimTypes.Email, kullanici.Eposta),
                    new Claim(ClaimTypes.Role, kullanici.Rol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Oturum açma işlemi
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View();
        }

        // GET: /Hesap/Cikis
        // GET: /Hesap/Profil
        [Authorize]
        public IActionResult Profil()
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var kullanici = _context.Kullanicilar.Find(userId);
            if (kullanici == null) return NotFound();

            return View(kullanici);
        }

        // POST: /Hesap/Profil
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profil(Kullanici model)
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var kullanici = _context.Kullanicilar.Find(userId);

            if (kullanici != null)
            {
                // Bilgileri güncelle
                kullanici.AdSoyad = model.AdSoyad;
                kullanici.Eposta = model.Eposta;
                if (!string.IsNullOrEmpty(model.Sifre))
                {
                    kullanici.Sifre = model.Sifre;
                }

                _context.SaveChanges();

                // OTURUMU YENİLE (Yeni bilgilerle tekrar giriş yap)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, kullanici.AdSoyad),
                    new Claim(ClaimTypes.Email, kullanici.Eposta),
                    new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString()),
                    new Claim(ClaimTypes.Role, kullanici.Rol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                TempData["Mesaj"] = "Bilgileriniz başarıyla güncellendi.";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        public async Task<IActionResult> Cikis()
        {
            // Oturumu kapatma işlemi
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

    }
}
