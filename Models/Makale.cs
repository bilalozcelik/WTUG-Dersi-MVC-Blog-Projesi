using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogProjesi.Models
{
    // Veritabanındaki 'Makaleler' tablosunu temsil eden model sınıfı
    public class Makale
    {
        // Birincil anahtar (Primary Key). Her makaleyi eşsiz yapan ID değeri.
        [Key]
        public int MakaleId { get; set; }

        // Başlık alanı boş bırakılamaz ve maksimum 200 karakter olabilir.
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(200)]
        public string Baslik { get; set; } = string.Empty;

        // Makale içeriği metni tutar. Gerekli (Required) olarak işaretlenmiştir.
        [Required(ErrorMessage = "İçerik zorunludur.")]
        public string Icerik { get; set; } = string.Empty;

        // Resim URL'si zorunlu değil (string? - null olabilir), maksimum 500 karakter.
        [StringLength(500)]
        public string? ResimUrl { get; set; }
        
        // Makalenin kaç kez detay sayfasına girilip okunduğunu tutar
        public int OkunmaSayisi { get; set; } = 0;
        
        // Makaleye verilen beğeni sayısını tutar
        public int BegeniSayisi { get; set; } = 0;
        
        // Makale ilk oluşturulduğunda sistem saatini (O an) otomatik olarak atar.
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        // Foreign Key (Yabancı Anahtar)
        // Bu makalenin hangi kullanıcıya (yazara) ait olduğunu belirtmek için KullaniciId veritabanında tutulur.
        [ForeignKey("Kullanici")]
        public int KullaniciId { get; set; }

        // Navigation property (Gezinme Özelliği)
        // C# kodları içinde 'makale.Kullanici.AdSoyad' gibi yazarın bilgilerine kolayca ulaşabilmemizi (Join) sağlar.
        public Kullanici? Kullanici { get; set; }
    }
}
