using System.ComponentModel.DataAnnotations;

namespace WEBBERBERODEV.Models
{
    public class Hizmet
    {
        public int Id { get; set; }
        
        [Required]
        public string Ad { get; set; }
        [Required]
        [Range(1,480, ErrorMessage ="SureDakika 1 ile 480 arasında olmalıdır.")]
        public int SureDakika { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Fiyat { get; set; }

        
        public ICollection<CalisanHizmet> CalisanHizmetler { get; set; } = new List<CalisanHizmet>();
    }
}
