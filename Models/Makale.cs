using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogProjesi.Models
{
    public class Makale
    {
        [Key]
        public int MakaleId { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(200)]
        public string Baslik { get; set; } = string.Empty;

        [Required(ErrorMessage = "İçerik zorunludur.")]
        public string Icerik { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ResimUrl { get; set; }
        public int OkunmaSayisi { get; set; } = 0;
        public int BegeniSayisi { get; set; } = 0;
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        // Foreign Key
        [ForeignKey("Kullanici")]
        public int KullaniciId { get; set; }

        // Navigation property
        public Kullanici? Kullanici { get; set; }
    }
}
