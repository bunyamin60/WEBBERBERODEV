using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBERBERODEV.Models
{
    public class RandevuHizmet
    {
        public int RandevuId { get; set; }
        public Randevu Randevu { get; set; }

        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; }
    }
}
