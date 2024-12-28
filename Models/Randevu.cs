using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEBBERBERODEV.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public int CalisanId { get; set; }
        public Calisan Calisan { get; set; }

        public ICollection<RandevuHizmet> RandevuHizmetler { get; set; } = new List<RandevuHizmet>();

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime RandevuTarihi { get; set; }

        [DataType(DataType.Currency)]
        public decimal Fiyat { get; set; }

        [Required]
        public RandevuDurumu Durum { get; set; }

        [Required]
        public string KullaniciId { get; set; }
        public ApplicationUser Kullanici { get; set; }
    }

    public enum RandevuDurumu
    {
        Beklemede,
        Onaylandi,
        IptalEdildi
    }
}
