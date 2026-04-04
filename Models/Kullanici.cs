using System.ComponentModel.DataAnnotations;

namespace BlogProjesi.Models
{
    // Veritabanındaki 'Kullanicilar' tablosunu temsil eden model sınıfı
    public class Kullanici
    {
        // [Key] bu property'nin veritabanında Primary Key (Birincil Anahtar) olacağını belirtir.
        [Key]
        public int KullaniciId { get; set; }

        // [Required] bu alanın formda boş geçilemeyeceğini ve veritabanında "NOT NULL" olmasını sağlar.
        // Boş geçilirse ErrorMessage içerisindeki metni form ekranında kullanıcıya gösterir.
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        // [StringLength] veritabanında ayrılacak alanı (örn: NVARCHAR(100)) ve girilebilecek maksimum karakteri sınırlar.
        [StringLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        // [EmailAddress] girilen metnin geçerli bir e-posta formatında (örn: a@b.com) olup olmadığını denetler.
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Eposta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        public string Sifre { get; set; } = string.Empty;

        // Kullanıcının sistemdeki yetkisini belirten alan (Admin, Yonetici, Yazar). 
        // Yeni kayıt olanlar varsayılan olarak "Yazar" rolünü alır.
        [StringLength(20)]
        public string Rol { get; set; } = "Yazar";

        // Navigation property (Gezinme Özelliği)
        // Entity Framework'te tablolar arası ilişki kurmayı sağlar (Bire-Çok ilişki).
        // Bir kullanıcının birden fazla makalesi olabileceği için ICollection (Liste) tipindedir.
        public ICollection<Makale> Makaleler { get; set; } = new List<Makale>();
    }
}
