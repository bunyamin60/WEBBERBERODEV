using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBERBERODEV.Models
{
    public class Calisan
    {
        public int Id { get; set; }
        [Required]
        public string Ad { get; set; }
        [Required]
        public string Soyad { get; set; }
        [Required]
        public string Uzmanlik { get; set; }
        public bool AktifMi { get; set; }

        // Çalışan Identity tablosundaki hangi kullanıcıya karşılık geliyor?
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        [NotMapped]
        public string AdSoyad => $"{Ad} {Soyad}";
        // Çalışanın çalışma saatlerini temsil eden koleksiyon
        public ICollection<CalisanCalismaSaatleri> CalisanCalismaSaatleri { get; set; } = new List<CalisanCalismaSaatleri>();
        public ICollection<CalisanHizmet> CalisanHizmetler { get; set; } = new List<CalisanHizmet>();
    }
}
