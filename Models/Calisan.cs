using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

       
        public ICollection<CalisanHizmet> CalisanHizmetler { get; set; } = new List<CalisanHizmet>();
    }
}
