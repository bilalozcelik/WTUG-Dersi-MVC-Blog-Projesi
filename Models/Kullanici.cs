using System.ComponentModel.DataAnnotations;

namespace BlogProjesi.Models
{
    public class Kullanici
    {
        [Key]
        public int KullaniciId { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Eposta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        public string Sifre { get; set; } = string.Empty;

        // Rol: Admin, Yonetici, Yazar
        [StringLength(20)]
        public string Rol { get; set; } = "Yazar";

        // Navigation property
        public ICollection<Makale> Makaleler { get; set; } = new List<Makale>();
    }
}
