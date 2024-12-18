using System.ComponentModel.DataAnnotations;

namespace WEBBERBERODEV.Models
{
    public class Hizmet
    {
        public int Id { get; set; }
        public int SalonId { get; set; }
        [Required]
        public string Ad { get; set; }
        public int SureDakika { get; set; }
        public decimal Fiyat { get; set; }

        public Salon Salon { get; set; }
        public ICollection<CalisanHizmet> CalisanHizmetler { get; set; }
    }
}
